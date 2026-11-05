using System.Diagnostics;
using System.Runtime.InteropServices;
using RtMidi.Net;
using RtMidi.Net.Clients;
using RtMidi.Net.Enums;

// Package test for RtMidi.Net: a short "real use" session on the current platform.
//
// Usage: RtMidi.Net.PackageTest [--devices] [--loopback]
//   (no flags)  load the native library, print the RtMidi version and compiled APIs
//   --devices   also initialise the MIDI backend and list the ports
//   --loopback  open a virtual output port, connect a MidiInputClient to it, send
//               Note On/Note Off and check both arrive through OnMessageReceived.
//               Needs virtual port support: CoreMIDI (macOS) or ALSA with /dev/snd/seq.

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

    if (args.Contains("--devices") || args.Contains("--loopback"))
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
    MidiDeviceInfo? source = null;
    var stopwatch = Stopwatch.StartNew();
    while (source is null && stopwatch.Elapsed < TimeSpan.FromSeconds(5))
    {
        source = MidiManager.GetAvailableDevices()
            .FirstOrDefault(d => d.Type == MidiDeviceType.Input && d.Name.Contains(portName));
        if (source is null) await Task.Delay(100);
    }
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

    lock (received)
    {
        if (received[0] is not MidiMessageNote { Type: MidiMessageType.NoteOn, Velocity: 100 } on || on.Note.GetByteRepresentation() != 60)
            return Fail($"first message should be Note On 60/100, got {received[0]}");
        if (received[1] is not MidiMessageNote { Type: MidiMessageType.NoteOff } off || off.Note.GetByteRepresentation() != 60)
            return Fail($"second message should be Note Off 60, got {received[1]}");
    }

    Console.WriteLine("  loopback OK");
    return 0;
}

static int Fail(string message)
{
    Console.Error.WriteLine($"FAILED: {message}");
    return 1;
}
