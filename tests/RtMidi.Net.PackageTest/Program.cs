using System.Runtime.InteropServices;
using RtMidi.Net;
using RtMidi.Net.Clients;
using RtMidi.Net.Enums;
using RtMidi.Net.PackageTest;

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
    return await Run(args);
}
catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
{
    return Ports.Fail($"native library could not be loaded: {ex}");
}
catch (RtMidiException ex)
{
    return Ports.Fail($"RtMidi error: {ex.Message}");
}

static async Task<int> Run(string[] args)
{
    var version = MidiManager.GetRtMidiVersion();
    Console.WriteLine($"RtMidi:   {version}");
    if (string.IsNullOrWhiteSpace(version))
        return Ports.Fail("empty RtMidi version");

    var apis = MidiManager.GetAvailableApis();
    Console.WriteLine($"APIs:     {string.Join(", ", apis.Select(MidiManager.GetApiDisplayName))}");
    if (apis.Count == 0)
        return Ports.Fail("no MIDI API compiled into the native library");

    var stressDuration = StressDuration(args);
    if (args.Contains("--devices") || args.Contains("--loopback") || stressDuration is not null)
        PrintDevices();

    var result = args.Contains("--loopback") ? await RunLoopback() : 0;
    if (result == 0 && stressDuration is { } duration)
        result = await StressTest.Run(duration);
    if (result != 0) return result;

    Console.WriteLine("OK");
    return 0;
}

// --stress [seconds]: null without the flag, 10 s when no time is given.
static TimeSpan? StressDuration(string[] args)
{
    var index = Array.IndexOf(args, "--stress");
    if (index < 0) return null;
    var seconds = index + 1 < args.Length && int.TryParse(args[index + 1], out var parsed) ? parsed : 10;
    return TimeSpan.FromSeconds(seconds);
}

static void PrintDevices()
{
    var devices = MidiManager.GetAvailableDevices();
    Console.WriteLine($"Devices:  {devices.Count}");
    foreach (var device in devices)
        Console.WriteLine($"  [{device.Type}] {device.Port}: {device.Name} ({device.Api})");
}

static async Task<int> RunLoopback()
{
    const string portName = "RtMidi.Net Loopback";
    Console.WriteLine("Loopback:");

    // 1. Virtual output port that the input client can connect to.
    using var output = new MidiOutputClient();
    output.OpenVirtual(portName);
    Console.WriteLine($"  opened virtual output port '{portName}'");

    // 2. Find it among the input ports (CoreMIDI may take a moment to publish it).
    var source = await Ports.FindInput(portName);
    if (source is null)
        return Ports.Fail($"virtual port '{portName}' not found among input ports");
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
        return Ports.Fail($"timed out waiting for messages, received {received.Count} of 2");

    string? error;
    lock (received) error = CheckNoteOnOff(received, note);
    if (error is not null)
        return Ports.Fail(error);

    Console.WriteLine("  loopback OK");
    return 0;
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
