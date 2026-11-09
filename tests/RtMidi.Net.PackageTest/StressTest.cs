using System.Collections.Concurrent;
using RtMidi.Net;
using RtMidi.Net.Clients;
using RtMidi.Net.Enums;
using RtMidi.Net.Events;

namespace RtMidi.Net.PackageTest;

/// <summary>
/// Sends Note On/Off in a loop through a virtual port while input clients connected
/// to it subscribe to the event and unsubscribe, poll, close and are disposed in every state.
/// Fails on a crash, an exception or a message that was not sent.
/// </summary>
/// <remarks>
/// The variations (when the event is activated, how long a client lives, how it is
/// released, which handler calls are slow) follow fixed patterns of the counters, so
/// a failing run can be repeated.
/// </remarks>
internal sealed class StressTest
{
    private const string PortName = "RtMidi.Net Stress";
    private const byte NoteNumber = 60;

    private readonly ConcurrentQueue<string> _errors = new();
    private long _sent;
    private long _viaEvent;
    private long _viaPolling;
    private long _clients;
    private long _toggles;

    public static async Task<int> Run(TimeSpan duration)
    {
        Console.WriteLine($"Stress ({duration.TotalSeconds:F0} s):");

        using var output = new MidiOutputClient();
        output.OpenVirtual(PortName);
        var source = await Ports.FindInput(PortName);
        if (source is null)
            return Ports.Fail($"virtual port '{PortName}' not found among input ports");
        Console.WriteLine($"  sending through input port {source.Port}: {source.Name}");

        var test = new StressTest();
        using var stop = new CancellationTokenSource(duration);
        var sender = Task.Factory.StartNew(() => test.Send(output, stop.Token), TaskCreationOptions.LongRunning);
        test.Receive(source, stop.Token);
        await stop.CancelAsync();
        await sender;
        output.Close();

        return test.Report();
    }

    /// <summary>
    /// Note On/Off pairs in bursts, as fast as the backend takes them.
    /// </summary>
    private void Send(MidiOutputClient output, CancellationToken stop)
    {
        var note = new MidiNote(NoteNumber);
        var noteOn = new MidiMessageNote(MidiChannel.Channel01, note, 100, MidiMessageType.NoteOn);
        var noteOff = new MidiMessageNote(MidiChannel.Channel01, note, 0, MidiMessageType.NoteOff);
        try
        {
            while (!stop.IsCancellationRequested)
            {
                for (var i = 0; i < 16; i++)
                {
                    output.SendMessage(noteOn);
                    output.SendMessage(noteOff);
                    _sent += 2;
                }
                Thread.Sleep(1);
            }
        }
        catch (Exception ex)
        {
            _errors.Enqueue($"sender: {ex}");
        }
    }

    /// <summary>
    /// Input clients that live for a few hundred event on/off cycles each.
    /// </summary>
    private void Receive(MidiDeviceInfo source, CancellationToken stop)
    {
        try
        {
            while (!stop.IsCancellationRequested && _errors.IsEmpty)
            {
                RunClient(source, ++_clients, stop);
            }
        }
        catch (Exception ex)
        {
            _errors.Enqueue($"receiver: {ex}");
        }
    }

    private void RunClient(MidiDeviceInfo source, long client, CancellationToken stop)
    {
        using var input = new MidiInputClient(source);
        var subscribed = client % 2 == 0;
        if (subscribed) input.MessageReceived += OnMessage; // before or after opening
        input.Open();

        var cycles = 50 + client * 37 % 250;
        for (var i = 0; i < cycles && !stop.IsCancellationRequested; i++)
        {
            if (!subscribed) input.MessageReceived += OnMessage;
            if (i % 4 == 0) Thread.Sleep(1);
            input.MessageReceived -= OnMessage;
            subscribed = false;
            _toggles++;
            Poll(input);
        }

        // Disposed with the event on, after closing, or with the event off.
        if (client % 3 == 0) input.MessageReceived += OnMessage;
        else if (client % 3 == 1) input.Close();
    }

    private void OnMessage(object? sender, MidiMessageReceivedEventArgs e)
    {
        // A slow handler now and then, so release has to wait for a running callback.
        if (Interlocked.Increment(ref _viaEvent) % 64 == 0) Thread.SpinWait(2000);
        Check(e.Message);
    }

    private void Poll(MidiInputClient input)
    {
        while (input.TryGetMessage(out var message, out _))
        {
            _viaPolling++;
            Check(message);
        }
    }

    private void Check(MidiMessage message)
    {
        if (message is not MidiMessageNote { Type: MidiMessageType.NoteOn or MidiMessageType.NoteOff } note
            || note.Note.Number != NoteNumber)
            _errors.Enqueue($"unexpected message {message}");
    }

    private int Report()
    {
        Console.WriteLine($"  sent {_sent} messages; received {_viaEvent} through the event and {_viaPolling} by polling");
        Console.WriteLine($"  {_clients} input clients, {_toggles} event on/off cycles");

        if (!_errors.IsEmpty)
            return Ports.Fail(string.Join(Environment.NewLine, _errors.Take(5)));
        if (_viaEvent == 0 || _viaPolling == 0)
            return Ports.Fail("expected messages through both the event and polling");

        Console.WriteLine("  stress OK");
        return 0;
    }
}
