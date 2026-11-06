using System.Diagnostics;
using System.Runtime.InteropServices;
using RtMidi.Net;
using RtMidi.Net.Clients;
using RtMidi.Net.Enums;

// Package test for RtMidi.Net: a short "real use" session on the current platform.
//
// Usage: RtMidi.Net.PackageTest [--devices] [--loopback] [--stress [seconds]]
//   (no flags)  load the native library, print the RtMidi version and compiled APIs
//   --devices   also initialise the MIDI backend and list the ports
//   --loopback  open a virtual output port, connect a MidiInputClient to it, send
//               Note On/Note Off and check both arrive through OnMessageReceived.
//   --stress    send messages in a loop through a virtual port while input clients
//               turn the event on and off, poll, close and are disposed, for the given
//               time (default 10 s). Fails on a crash, an exception or a wrong message.
//   --loopback and --stress need virtual ports: CoreMIDI (macOS) or ALSA with /dev/snd/seq.

Console.WriteLine($"Runtime:  {RuntimeInformation.RuntimeIdentifier} ({RuntimeInformation.OSDescription})");

try
{
    var version = MidiManager.GetRtMidiVersion();
    Console.WriteLine($"RtMidi:   {version}");
    if (string.IsNullOrWhiteSpace(version))
        return Fail("empty RtMidi version");

    var apis = MidiManager.GetAvailableApis();
    Console.WriteLine($"APIs:     {string.Join(", ", apis.Select(MidiManager.GetApiDisplayName))}");
    if (apis.Count == 0)
        return Fail("no MIDI API compiled into the native library");

    if (args.Contains("--devices") || args.Contains("--loopback") || args.Contains("--stress"))
    {
        var devices = MidiManager.GetAvailableDevices();
        Console.WriteLine($"Devices:  {devices.Count}");
        foreach (var device in devices)
            Console.WriteLine($"  [{device.Type}] {device.Port}: {device.Name} ({device.Api})");
    }

    if (args.Contains("--loopback"))
    {
        var result = await RunLoopback();
        if (result != 0) return result;
    }

    var stressIndex = Array.IndexOf(args, "--stress");
    if (stressIndex >= 0)
    {
        var seconds = stressIndex + 1 < args.Length && int.TryParse(args[stressIndex + 1], out var parsed) ? parsed : 10;
        var result = await RunStress(TimeSpan.FromSeconds(seconds));
        if (result != 0) return result;
    }
}
catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
{
    return Fail($"native library could not be loaded: {ex}");
}
catch (RtMidiException ex)
{
    return Fail($"RtMidi error: {ex.Message}");
}

Console.WriteLine("OK");
return 0;

static async Task<int> RunLoopback()
{
    const string portName = "RtMidi.Net Loopback";
    Console.WriteLine("Loopback:");

    // 1. Virtual output port that the input client can connect to.
    using var output = new MidiOutputClient();
    output.OpenVirtual(portName);
    Console.WriteLine($"  opened virtual output port '{portName}'");

    // 2. Find it among the input ports (CoreMIDI may take a moment to publish it).
    var source = await FindInputPort(portName);
    if (source is null)
        return Fail($"virtual port '{portName}' not found among input ports");
    Console.WriteLine($"  found it as input port {source.Port}: {source.Name}");

    // 3. Input client with the event enabled.
    var received = new List<MidiMessage>();
    var gotBoth = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    using var input = new MidiInputClient(source);
    input.OnMessageReceived += (_, e) =>
    {
        Console.WriteLine($"  received {e.Message} at {e.Timestamp.TotalMilliseconds:F1} ms");
        lock (received)
        {
            received.Add(e.Message);
            if (received.Count >= 2) gotBoth.TrySetResult();
        }
    };
    input.ActivateMessageReceivedEvent();
    input.Open();
    Console.WriteLine("  input client open, events active");

    // 4. Send Note On + Note Off for middle C (60) on channel 1.
    var note = new MidiNote(60);
    var noteOn = new MidiMessageNote(MidiChannel.Channel01, note, 100, MidiMessageType.NoteOn);
    var noteOff = new MidiMessageNote(MidiChannel.Channel01, note, 0, MidiMessageType.NoteOff);
    output.SendMessage(noteOn);
    output.SendMessage(noteOff);
    Console.WriteLine($"  sent {noteOn}");
    Console.WriteLine($"  sent {noteOff}");

    var finished = await Task.WhenAny(gotBoth.Task, Task.Delay(TimeSpan.FromSeconds(5)));

    // 5. Close everything.
    input.DeactivateMessageReceivedEvent();
    input.Close();
    output.Close();
    Console.WriteLine("  closed input and output");

    if (finished != gotBoth.Task)
        return Fail($"timed out waiting for messages, received {received.Count} of 2");

    string? error;
    lock (received) error = CheckNoteOnOff(received, note);
    if (error is not null)
        return Fail(error);

    Console.WriteLine("  loopback OK");
    return 0;
}

static async Task<int> RunStress(TimeSpan duration)
{
    const string portName = "RtMidi.Net Stress";
    Console.WriteLine($"Stress ({duration.TotalSeconds:F0} s):");

    using var output = new MidiOutputClient();
    output.OpenVirtual(portName);
    var source = await FindInputPort(portName);
    if (source is null)
        return Fail($"virtual port '{portName}' not found among input ports");
    Console.WriteLine($"  sending through input port {source.Port}: {source.Name}");

    long sent = 0, viaEvent = 0, viaPolling = 0, clients = 0, toggles = 0;
    var errors = new System.Collections.Concurrent.ConcurrentQueue<string>();
    using var stop = new CancellationTokenSource(duration);

    // Sender: Note On/Off pairs in bursts, as fast as the backend takes them.
    var sender = Task.Factory.StartNew(() =>
    {
        var noteOn = new MidiMessageNote(MidiChannel.Channel01, new MidiNote(60), 100, MidiMessageType.NoteOn);
        var noteOff = new MidiMessageNote(MidiChannel.Channel01, new MidiNote(60), 0, MidiMessageType.NoteOff);
        try
        {
            while (!stop.IsCancellationRequested)
            {
                for (var i = 0; i < 16; i++)
                {
                    output.SendMessage(noteOn);
                    output.SendMessage(noteOff);
                    sent += 2;
                }
                Thread.Sleep(1);
            }
        }
        catch (Exception ex)
        {
            errors.Enqueue($"sender: {ex}");
        }
    }, TaskCreationOptions.LongRunning);

    void Check(MidiMessage message)
    {
        if (message is not MidiMessageNote { Type: MidiMessageType.NoteOn or MidiMessageType.NoteOff } note
            || note.Note.GetByteRepresentation() != 60)
            errors.Enqueue($"unexpected message {message}");
    }

    // Receiver: clients that live for a few hundred toggles each and are disposed
    // in every state (event on or off, port open or already closed).
    var random = new Random(1234);
    try
    {
        while (!stop.IsCancellationRequested && errors.IsEmpty)
        {
            var input = new MidiInputClient(source);
            clients++;
            input.OnMessageReceived += (_, e) =>
            {
                Interlocked.Increment(ref viaEvent);
                Check(e.Message);
                if (Random.Shared.Next(64) == 0) Thread.SpinWait(2000); // a slow handler now and then
            };
            if (random.Next(2) == 0) input.ActivateMessageReceivedEvent(); // before or after opening
            input.Open();

            var cycles = random.Next(50, 300);
            for (var i = 0; i < cycles && !stop.IsCancellationRequested; i++)
            {
                input.ActivateMessageReceivedEvent();
                if (random.Next(4) == 0) Thread.Sleep(1);
                input.DeactivateMessageReceivedEvent();
                toggles++;
                while (input.TryGetMessage(out var message, out _))
                {
                    viaPolling++;
                    Check(message);
                }
            }

            switch (random.Next(3))
            {
                case 0: input.ActivateMessageReceivedEvent(); break; // dispose with the event on
                case 1: input.Close(); break;                        // dispose after closing
            }
            input.Dispose();

            if (clients % 20 == 0) GC.Collect();
        }
    }
    catch (Exception ex)
    {
        errors.Enqueue($"receiver: {ex}");
    }

    await stop.CancelAsync();
    await sender;
    output.Close();

    Console.WriteLine($"  sent {sent} messages; received {viaEvent} through the event and {viaPolling} by polling");
    Console.WriteLine($"  {clients} input clients, {toggles} event on/off cycles");

    if (!errors.IsEmpty)
        return Fail(string.Join(Environment.NewLine, errors.Take(5)));
    if (viaEvent == 0 || viaPolling == 0)
        return Fail("expected messages through both the event and polling");

    Console.WriteLine("  stress OK");
    return 0;
}

static async Task<MidiDeviceInfo?> FindInputPort(string portName)
{
    var stopwatch = Stopwatch.StartNew();
    while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
    {
        var port = MidiManager.GetAvailableDevices()
            .FirstOrDefault(d => d.Type == MidiDeviceType.Input && d.Name.Contains(portName));
        if (port is not null) return port;
        await Task.Delay(100);
    }
    return null;
}

static string? CheckNoteOnOff(IReadOnlyList<MidiMessage> received, MidiNote note)
{
    var expected = note.GetByteRepresentation();
    if (received[0] is not MidiMessageNote { Type: MidiMessageType.NoteOn, Velocity: 100 } on
        || on.Note.GetByteRepresentation() != expected)
        return $"first message should be Note On {expected}/100, got {received[0]}";
    if (received[1] is not MidiMessageNote { Type: MidiMessageType.NoteOff } off
        || off.Note.GetByteRepresentation() != expected)
        return $"second message should be Note Off {expected}, got {received[1]}";
    return null;
}

static int Fail(string message)
{
    Console.Error.WriteLine($"FAILED: {message}");
    return 1;
}
