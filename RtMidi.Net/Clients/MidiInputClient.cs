using System.Diagnostics.CodeAnalysis;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net;

/// <summary>
/// Receives MIDI messages from an input port, through <see cref="MessageReceived"/> or by polling.
/// </summary>
/// <remarks>
/// While <see cref="MessageReceived"/> has no handlers, messages are queued and read with
/// <see cref="TryGetMessage"/>, <see cref="GetMessage(CancellationToken)"/> or <see cref="GetMessageAsync"/>.
/// System Exclusive, timing and Active Sensing messages are ignored by default.
/// </remarks>
/// <example>
/// <code>
/// using var input = new MidiInputClient(device);
/// input.MessageReceived += (_, e) =&gt;
/// {
///     if (e.Message is MidiMessageNote { Type: MidiMessageType.NoteOn, Velocity: &gt; 0 } note)
///         Console.WriteLine($"{note.Note} velocity {note.Velocity}"); // e.g. "C4 velocity 100"
/// };
/// input.Open();
/// </code>
/// </example>
public sealed class MidiInputClient : MidiClient
{
    private readonly RtMidiIn _rtMidiInClient;
    private readonly object _eventLock = new();
    private EventHandler<MidiMessageReceivedEventArgs>? _messageReceived;
    private bool _disposed; // guarded by _eventLock, so no subscription change reaches a freed native object
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
    /// Raised on RtMidi's thread for every message received while it has at least one handler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Subscribing starts delivering messages here instead of the queue read by the polling methods;
    /// removing the last handler sends new messages to the queue again. Messages already queued
    /// when the first handler is added stay in the queue. Subscribe before <see cref="MidiClient.Open"/>
    /// so no message is queued in between.
    /// </para>
    /// <para>
    /// Subscribing after <see cref="MidiClient.Dispose()"/> throws <see cref="ObjectDisposedException"/>;
    /// unsubscribing is always allowed.
    /// </para>
    /// <para>
    /// The timestamp is the delta-time since the previous message. An exception thrown
    /// by a handler is caught and written to <see cref="System.Diagnostics.Trace"/>,
    /// because it can't propagate through the native library.
    /// </para>
    /// </remarks>
    public event EventHandler<MidiMessageReceivedEventArgs>? MessageReceived
    {
        add
        {
            lock (_eventLock)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                var previous = _messageReceived;
                _messageReceived += value;
                if (ReferenceEquals(previous, _messageReceived)) return;
                try
                {
                    UpdateCallback();
                }
                catch
                {
                    _messageReceived = previous;
                    throw;
                }
            }
        }
        remove
        {
            lock (_eventLock)
            {
                _messageReceived -= value;
                // After disposal only the managed list changes: the native object is gone.
                if (!_disposed) UpdateCallback();
            }
        }
    }

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
    /// <param name="deviceInfo">An input port from <see cref="MidiManager"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="deviceInfo"/> is null.</exception>
    public MidiInputClient(MidiDeviceInfo deviceInfo) : base(deviceInfo, new RtMidiIn())
    {
        _rtMidiInClient = (RtMidiIn)RtMidiClient;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // Mark the client as disposed under the event lock, so a concurrent subscription change either
        // finishes before the native object is freed or sees the flag. The native object is freed
        // outside the lock: a handler running on RtMidi's thread may unsubscribe while Dispose waits
        // for that thread to stop.
        lock (_eventLock)
        {
            _disposed = true;
        }

        base.Dispose(disposing);
    }

    private void UpdateIgnoredTypes()
    {
        _rtMidiInClient.IgnoreTypes(_ignoreSysexMessages, _ignoreTimeMessages, _ignoreSenseMessages);
    }

    // The native handler captures the current handlers, so a message already being delivered when
    // the last handler is removed still reaches them instead of being lost (it can't go to the queue
    // any more). Without handlers, new messages go to the queue.
    private void UpdateCallback()
    {
        if (_messageReceived is not { } handlers)
        {
            _rtMidiInClient.CancelCallback();
            return;
        }

        _rtMidiInClient.SetCallback((timestamp, messageData) =>
        {
            var message = MidiMessageConverter.Parse(messageData);
            handlers(this, new MidiMessageReceivedEventArgs(message, TimeSpan.FromSeconds(timestamp)));
        });
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
    /// Waits for the next message from the input queue, blocking the calling thread.
    /// </summary>
    /// <param name="cancellationToken">Stops waiting.</param>
    /// <returns>The message and its delta-time since the previous message.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before a message arrived.</exception>
    public (MidiMessage Message, TimeSpan Timestamp) GetMessage(CancellationToken cancellationToken = default)
    {
        return MessagePolling.Wait(_rtMidiInClient.GetMessage, cancellationToken);
    }

    /// <summary>
    /// Waits for the next message from the input queue without blocking a thread.
    /// </summary>
    /// <param name="cancellationToken">Stops waiting.</param>
    /// <returns>The message and its delta-time since the previous message.</returns>
    /// <exception cref="OperationCanceledException">The token was cancelled before a message arrived.</exception>
    public Task<(MidiMessage Message, TimeSpan Timestamp)> GetMessageAsync(CancellationToken cancellationToken = default)
    {
        return MessagePolling.WaitAsync(_rtMidiInClient.GetMessage, cancellationToken);
    }
}
