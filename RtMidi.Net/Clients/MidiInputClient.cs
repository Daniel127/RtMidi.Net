using System.Diagnostics.CodeAnalysis;
using RtMidi.Net.Events;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Clients;

/// <summary>
/// Receives MIDI messages from an input port, through <see cref="OnMessageReceived"/> or by polling.
/// </summary>
public class MidiInputClient : MidiClient
{
    private readonly RtMidiIn _rtMidiInClient;
    private bool _ignoreSysexMessages = true;
    private bool _ignoreTimeMessages = true;
    private bool _ignoreSenseMessages = true;

    /// <summary>
    /// Ignore System Exclusive messages (default true).
    /// </summary>
    public bool IgnoreSysexMessages
    {
        get => _ignoreSysexMessages;
        set
        {
            _ignoreSysexMessages = value;
            UpdateIgnoredTypes();
        }
    }

    /// <summary>
    /// Ignore timing messages: MIDI Time Code and Timing Clock (default true).
    /// </summary>
    public bool IgnoreTimeMessages
    {
        get => _ignoreTimeMessages;
        set
        {
            _ignoreTimeMessages = value;
            UpdateIgnoredTypes();
        }
    }

    /// <summary>
    /// Ignore Active Sensing messages (default true).
    /// </summary>
    public bool IgnoreSenseMessages
    {
        get => _ignoreSenseMessages;
        set
        {
            _ignoreSenseMessages = value;
            UpdateIgnoredTypes();
        }
    }

    /// <summary>
    /// Raised on RtMidi's thread for every message, after <see cref="ActivateMessageReceivedEvent"/>.
    /// </summary>
    /// <remarks>
    /// The timestamp is the delta-time since the previous message. An exception thrown
    /// by a handler is caught and written to <see cref="System.Diagnostics.Trace"/>,
    /// because it can't propagate through the native library.
    /// </remarks>
    public event EventHandler<MidiMessageReceivedEventArgs>? OnMessageReceived;

    /// <summary>
    /// Creates a client that is not bound to a device, to be used with
    /// <see cref="MidiClient.OpenVirtual"/>.
    /// </summary>
    public MidiInputClient() : base(new RtMidiIn())
    {
        _rtMidiInClient = (RtMidiIn)RtMidiClient;
    }

    /// <summary>
    /// Creates a client for the given input device. Call <see cref="MidiClient.Open"/> to connect.
    /// </summary>
    public MidiInputClient(MidiDeviceInfo deviceInfo) : base(deviceInfo, new RtMidiIn())
    {
        _rtMidiInClient = (RtMidiIn)RtMidiClient;
    }

    /// <summary>
    /// Starts raising <see cref="OnMessageReceived"/>. While it is active, the polling methods receive nothing.
    /// </summary>
    public void ActivateMessageReceivedEvent()
    {
        _rtMidiInClient.SetCallback(InternalMessageReceived);
    }

    /// <summary>
    /// Stops raising <see cref="OnMessageReceived"/>; new messages go to the queue read by the polling methods.
    /// </summary>
    public void DeactivateMessageReceivedEvent()
    {
        _rtMidiInClient.CancelCallback();
    }

    private void UpdateIgnoredTypes()
    {
        _rtMidiInClient.IgnoreTypes(_ignoreSysexMessages, _ignoreTimeMessages, _ignoreSenseMessages);
    }

    private void InternalMessageReceived(double timestamp, ReadOnlySpan<byte> messageData)
    {
        var message = MidiMessageConverter.Parse(messageData);
        OnMessageReceived?.Invoke(this, new MidiMessageReceivedEventArgs(message, TimeSpan.FromSeconds(timestamp)));
    }

    /// <summary>
    /// Takes the next message from the input queue, if there is one. Returns immediately.
    /// </summary>
    /// <param name="message">The message, or null if the queue is empty.</param>
    /// <param name="timestamp">Delta-time since the previous message.</param>
    /// <returns>true if a message was read.</returns>
    public bool TryGetMessage([NotNullWhen(true)] out MidiMessage? message, out TimeSpan timestamp)
    {
        return MessagePolling.TryRead(_rtMidiInClient.GetMessage, out message, out timestamp);
    }

    /// <summary>
    /// Waits for the next message from the input queue.
    /// </summary>
    /// <returns>The message and its delta-time since the previous message.</returns>
    public (MidiMessage, TimeSpan) GetMessage()
    {
        return MessagePolling.Wait(_rtMidiInClient.GetMessage, CancellationToken.None);
    }

    /// <summary>
    /// Waits for the next message from the input queue without blocking a thread.
    /// </summary>
    /// <param name="cancellationToken">Stops waiting.</param>
    /// <returns>The message and its delta-time since the previous message.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before a message arrived.</exception>
    public Task<(MidiMessage, TimeSpan)> GetMessageAsync(CancellationToken cancellationToken)
    {
        return MessagePolling.WaitAsync(_rtMidiInClient.GetMessage, cancellationToken);
    }
}
