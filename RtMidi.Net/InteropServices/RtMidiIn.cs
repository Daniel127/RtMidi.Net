using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RtMidi.Net.InteropServices;

/// <summary>
/// Handler for messages received through the RtMidi callback.
/// </summary>
/// <param name="timestamp">Delta-time in seconds since the previous message.</param>
/// <param name="message">The message bytes, only valid during the call.</param>
internal delegate void RtMidiMessageHandler(double timestamp, ReadOnlySpan<byte> message);

/// <summary>
/// <c>RtMidiCCallback</c>: <c>void (*)(double timeStamp, const unsigned char* message, size_t messageSize, void* userData)</c>.
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void RtMidiNativeCallback(double timestamp, IntPtr message, nuint size, IntPtr userData);

/// <summary>
/// This class provides a common, platform-independent API for
/// realtime MIDI input.  It allows access to a single MIDI input
/// port.  Incoming MIDI messages are either saved to a queue for
/// retrieval using the getMessage() function or immediately passed to
/// a user-specified callback function.  Create multiple instances of
/// this class to connect to more than one MIDI device at the same
/// time.  With the OS-X, Linux ALSA, and JACK MIDI APIs, it is also
/// possible to open a virtual input port to which other MIDI software
/// clients can connect.
/// </summary>
internal sealed class RtMidiIn : RtMidiBase
{
    /// <summary>
    /// Initial size of the buffer used by <see cref="GetMessage()"/>. It grows when a
    /// longer message (a SysEx dump) is queued.
    /// </summary>
    internal const int InitialMessageBufferSize = 1024;

    /// <summary>
    /// Queue size limit RtMidi uses when none is given.
    /// </summary>
    internal const uint DefaultQueueSizeLimit = 100;

    // One static delegate for every instance (the instance travels in userData), kept
    // alive by this field for the lifetime of the process.
    private static readonly RtMidiNativeCallback NativeCallbackDelegate = OnNativeMessage;
    private static readonly IntPtr NativeCallbackPointer = Marshal.GetFunctionPointerForDelegate(NativeCallbackDelegate);

    // RtMidi gets a token as userData instead of a GCHandle:
    // - the registry holds weak references, so a registered callback does not keep an
    //   undisposed instance (and its client and event subscribers) alive;
    // - a callback that is already running when the instance is released may still arrive
    //   with an old userData. Tokens are never reused and an unknown token is ignored,
    //   unlike a freed GCHandle, which could crash or reach another object.
    private static readonly ConcurrentDictionary<nint, WeakReference<RtMidiIn>> CallbackRegistry = new();
    private static long _lastCallbackToken;

    // The native callback is set once and never cancelled while the port is in use:
    // rtmidi_in_cancel_callback (RtMidi 6.1) frees the data its input thread reads to
    // call us, so cancelling while a message arrives could crash. Instead,
    // CancelCallback only removes the handler, and messages that arrive without one go
    // to _pendingMessages, which GetMessage reads after the native queue.
    private volatile RtMidiMessageHandler? _handler;
    private nint _callbackToken;
    private readonly uint _queueSizeLimit;
    private readonly ConcurrentQueue<(byte[] Message, double Timestamp)> _pendingMessages = new();
    private readonly object _messageBufferLock = new();
    private byte[]? _messageBuffer;

    /// <summary>
    /// Default constructor
    /// </summary>
    public RtMidiIn() : base(EnsureCreated(RtMidiInterop.rtmidi_in_create_default(), RtMidiInterop.rtmidi_in_free))
    {
        _queueSizeLimit = DefaultQueueSizeLimit;
    }

    /// <summary>
    /// <para>Constructor that allows an optional api, client name and queue size.</para>
    /// <para>
    /// The queue size defines the maximum number of
    /// messages that can be held in the MIDI queue (when not using a
    /// callback function).  If the queue size limit is reached,
    /// incoming messages will be ignored.
    /// </para>
    /// </summary>
    /// <remarks>An exception will be thrown if a MIDI system initialization
    /// error occurs.</remarks>
    public RtMidiIn(MidiApi api, string clientName, uint queueSizeLimit = DefaultQueueSizeLimit)
        : base(EnsureCreated(RtMidiInterop.rtmidi_in_create((int)api, clientName, queueSizeLimit), RtMidiInterop.rtmidi_in_free))
    {
        _queueSizeLimit = queueSizeLimit;
    }

    /// <inheritdoc />
    protected override string DefaultPortName => "RtMidi Input";

    /// <inheritdoc />
    public override MidiApi GetCurrentApi()
    {
        return (MidiApi)RtMidiInterop.rtmidi_in_get_current_api(RtMidiPtr);
    }

    /// <summary>
    /// Set a handler to be invoked for incoming MIDI messages.
    /// </summary>
    /// <remarks>
    /// The handler is called on RtMidi's thread whenever a message is received.
    /// While not absolutely necessary, it is best to set it before opening a
    /// MIDI port to avoid leaving some messages in the queue.
    /// </remarks>
    public void SetCallback(RtMidiMessageHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
        if (_callbackToken != 0) return;

        // While a callback is set RtMidi stops queueing and warns on every read, so take
        // what is already queued first; GetMessage returns it before newer messages.
        lock (_messageBufferLock)
        {
            while (true)
            {
                var (message, timestamp) = ReadNativeMessage(ReadQueuedMessage);
                if (message.IsEmpty) break;
                _pendingMessages.Enqueue((message.ToArray(), timestamp));
            }
        }

        var token = RegisterCallbackTarget(this);
        RtMidiInterop.rtmidi_in_set_callback(RtMidiPtr, NativeCallbackPointer, token);
        if (!LastCallSucceeded)
        {
            CallbackRegistry.TryRemove(token, out _);
            _handler = null;
            ThrowIfError();
        }

        _callbackToken = token;
    }

    /// <summary>
    /// Cancel use of the current callback (if one exists).
    /// <para>
    /// Subsequent incoming MIDI messages will be written to the queue
    /// and can be retrieved with the <see cref="GetMessage()"/> function.
    /// </para>
    /// </summary>
    public void CancelCallback()
    {
        _handler = null;
    }

    /// <summary>
    /// Specify whether certain MIDI message types should be queued or ignored during input.
    /// </summary>
    /// <remarks>
    /// By default, MIDI timing and active sensing messages are ignored
    /// during message input because of their relative high data rates.
    /// MIDI sysex messages are ignored by default as well.  Variable
    /// values of "true" imply that the respective message type will be
    /// ignored.
    /// </remarks>
    public void IgnoreTypes(bool midiSysex = true, bool midiTime = true, bool midiSense = true)
    {
        // Cannot fail, and unlike the other functions it does not reset the error status.
        RtMidiInterop.rtmidi_in_ignore_types(RtMidiPtr, midiSysex, midiTime, midiSense);
    }

    /// <summary>
    /// Takes the next available MIDI message from the input queue.
    /// </summary>
    /// <remarks>
    /// This function returns immediately whether a new message is available or not.
    /// </remarks>
    /// <returns>The message (empty if the queue is empty) and its delta-time in seconds.</returns>
    public (byte[] message, double timestamp) GetMessage() => GetMessage(ReadQueuedMessage);

    /// <summary>
    /// Reads the next queued message like <c>rtmidi_in_get_message</c> (RtMidi 6.1):
    /// copies it into <paramref name="buffer"/> and sets <paramref name="size"/> to its
    /// length (0 if the queue is empty). A message that does not fit stays queued, the
    /// call fails and <paramref name="size"/> is set to its length.
    /// </summary>
    internal delegate double QueuedMessageReader(byte[] buffer, ref nuint size);

    /// <summary>
    /// <see cref="GetMessage()"/> with the native read replaced (for tests).
    /// </summary>
    internal (byte[] message, double timestamp) GetMessage(QueuedMessageReader reader)
    {
        // Once the native callback is set, every message arrives through it.
        if (_callbackToken != 0)
        {
            return _pendingMessages.TryDequeue(out var pending) ? (pending.Message, pending.Timestamp) : ([], 0);
        }

        lock (_messageBufferLock)
        {
            var (message, timestamp) = ReadNativeMessage(reader);
            return (message.ToArray(), timestamp);
        }
    }

    /// <summary>
    /// Reads the next message of RtMidi's queue into the shared buffer (caller holds the lock).
    /// </summary>
    private (ReadOnlyMemory<byte> Message, double Timestamp) ReadNativeMessage(QueuedMessageReader reader)
    {
        _messageBuffer ??= new byte[InitialMessageBufferSize];
        var size = (nuint)_messageBuffer.Length;
        var timestamp = reader(_messageBuffer, ref size);

        if (size > (nuint)_messageBuffer.Length)
        {
            // Too long for the buffer: RtMidi keeps it queued, so read it again with room for it.
            _messageBuffer = new byte[checked((int)size)];
            size = (nuint)_messageBuffer.Length;
            timestamp = reader(_messageBuffer, ref size);
        }

        ThrowIfError();
        return size == 0 ? (ReadOnlyMemory<byte>.Empty, timestamp) : (_messageBuffer.AsMemory(0, (int)size), timestamp);
    }

    private double ReadQueuedMessage(byte[] buffer, ref nuint size) =>
        RtMidiInterop.rtmidi_in_get_message(RtMidiPtr, buffer, ref size);

    /// <summary>
    /// Called by RtMidi on its thread for every message (internal for tests).
    /// </summary>
    internal static void OnNativeMessage(double timestamp, IntPtr message, nuint size, IntPtr userData)
    {
        // An exception must never cross back into native code (it would terminate the process).
        try
        {
            if (userData == IntPtr.Zero || message == IntPtr.Zero) return;
            if (!CallbackRegistry.TryGetValue(userData, out var reference)) return;
            if (!reference.TryGetTarget(out var instance)) return;
            var data = new byte[checked((int)size)];
            Marshal.Copy(message, data, 0, data.Length);
            instance.Deliver(timestamp, data);
        }
        catch (Exception ex)
        {
            Trace.TraceError($"RtMidi.Net: unhandled exception in the MIDI message handler: {ex}");
        }
    }

    private void Deliver(double timestamp, byte[] data)
    {
        if (_handler is { } handler)
        {
            handler(timestamp, data);
            return;
        }

        if (_callbackToken == 0) return; // released

        // Same rule as RtMidi's own queue: when it is full, new messages are dropped.
        if (_pendingMessages.Count >= _queueSizeLimit)
        {
            Trace.TraceWarning("RtMidi.Net: MIDI message queue limit reached, message dropped.");
            return;
        }

        _pendingMessages.Enqueue((data, timestamp));
    }

    /// <summary>
    /// The user data RtMidi passes to the callback (for tests).
    /// </summary>
    internal IntPtr CallbackUserData => _callbackToken;

    private static nint RegisterCallbackTarget(RtMidiIn target)
    {
        var token = (nint)Interlocked.Increment(ref _lastCallbackToken);
        CallbackRegistry[token] = new WeakReference<RtMidiIn>(target);
        return token;
    }

    private void UnregisterCallback()
    {
        if (_callbackToken == 0) return;
        CallbackRegistry.TryRemove(_callbackToken, out _);
        _callbackToken = 0;
    }

    /// <inheritdoc />
    protected override void ReleaseUnmanagedResources(IntPtr rtMidiPtr)
    {
        // rtmidi_in_free frees the callback's data before it stops the input thread, so
        // stop the input first: closing the port ends delivery (ALSA joins its thread,
        // CoreMIDI disposes the port and the virtual endpoint).
        _handler = null;
        if (_callbackToken != 0) RtMidiInterop.rtmidi_close_port(rtMidiPtr);
        RtMidiInterop.rtmidi_in_free(rtMidiPtr);
        UnregisterCallback();
        _pendingMessages.Clear();
    }
}
