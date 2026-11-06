using RtMidi.Net.Enums;
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
    /// Size of the buffer used by <see cref="GetMessage"/>. RtMidi drops a queued
    /// message that does not fit, so it is large enough for long SysEx dumps.
    /// </summary>
    internal const int MessageBufferSize = 64 * 1024;

    // One static delegate for every instance (the instance travels in userData), kept
    // alive by this field for the lifetime of the process.
    private static readonly RtMidiNativeCallback NativeCallbackDelegate = OnNativeMessage;
    private static readonly IntPtr NativeCallbackPointer = Marshal.GetFunctionPointerForDelegate(NativeCallbackDelegate);

    // RtMidi gets a token as userData instead of a GCHandle:
    // - the registry holds weak references, so a registered callback does not keep an
    //   undisposed instance (and its client and event subscribers) alive;
    // - RtMidi 6.0 does not wait for a running callback when it is cancelled, so a late
    //   callback may still arrive with an old userData. Tokens are never reused and an
    //   unknown token is ignored, unlike a freed GCHandle, which could crash or reach
    //   another object.
    private static readonly ConcurrentDictionary<nint, WeakReference<RtMidiIn>> CallbackRegistry = new();
    private static long _lastCallbackToken;

    private RtMidiMessageHandler? _handler;
    private nint _callbackToken;
    private readonly object _messageBufferLock = new();
    private byte[]? _messageBuffer;

    /// <summary>
    /// Default constructor
    /// </summary>
    public RtMidiIn() : base(EnsureCreated(RtMidiInterop.rtmidi_in_create_default(), RtMidiInterop.rtmidi_in_free))
    {
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
    public RtMidiIn(MidiApi api, string clientName, uint queueSizeLimit = 100)
        : base(EnsureCreated(RtMidiInterop.rtmidi_in_create((int)api, clientName, queueSizeLimit), RtMidiInterop.rtmidi_in_free))
    {
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
        if (_callbackToken == 0)
        {
            _callbackToken = (nint)Interlocked.Increment(ref _lastCallbackToken);
            CallbackRegistry[_callbackToken] = new WeakReference<RtMidiIn>(this);
        }

        RtMidiInterop.rtmidi_in_set_callback(RtMidiPtr, NativeCallbackPointer, _callbackToken);
        ThrowIfError();
    }

    /// <summary>
    /// Cancel use of the current callback (if one exists).
    /// <para>
    /// Subsequent incoming MIDI messages will be written to the queue
    /// and can be retrieved with the <see cref="GetMessage"/> function.
    /// </para>
    /// </summary>
    public void CancelCallback()
    {
        RtMidiInterop.rtmidi_in_cancel_callback(RtMidiPtr);
        // The token stays registered until the instance is released: a callback that
        // was already running finds no handler and does nothing.
        _handler = null;
        ThrowIfError();
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
        RtMidiInterop.rtmidi_in_ignore_types(RtMidiPtr, midiSysex, midiTime, midiSense);
        ThrowIfError();
    }

    /// <summary>
    /// Takes the next available MIDI message from the input queue.
    /// </summary>
    /// <remarks>
    /// This function returns immediately whether a new message is available or not.
    /// </remarks>
    /// <returns>The message (empty if the queue is empty) and its delta-time in seconds.</returns>
    public (byte[] message, double timestamp) GetMessage()
    {
        lock (_messageBufferLock)
        {
            _messageBuffer ??= new byte[MessageBufferSize];
            var size = (nuint)_messageBuffer.Length;
            var timestamp = RtMidiInterop.rtmidi_in_get_message(RtMidiPtr, _messageBuffer, ref size);

            ThrowIfError();

            if (size == 0) return ([], timestamp);
            if (size > (nuint)_messageBuffer.Length)
            {
                // RtMidi already removed it from the queue without copying it.
                Trace.TraceWarning($"RtMidi.Net: dropped a {size} byte MIDI message larger than the {MessageBufferSize} byte buffer.");
                return ([], timestamp);
            }

            return (_messageBuffer.AsSpan(0, (int)size).ToArray(), timestamp);
        }
    }

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
            if (!reference.TryGetTarget(out var instance) || instance._handler is not { } handler) return;
            var data = new byte[checked((int)size)];
            Marshal.Copy(message, data, 0, data.Length);
            handler(timestamp, data);
        }
        catch (Exception ex)
        {
            Trace.TraceError($"RtMidi.Net: unhandled exception in the MIDI message handler: {ex}");
        }
    }

    /// <summary>
    /// The user data RtMidi passes to the callback (for tests).
    /// </summary>
    internal IntPtr CallbackUserData => _callbackToken;

    private void UnregisterCallback()
    {
        if (_callbackToken == 0) return;
        CallbackRegistry.TryRemove(_callbackToken, out _);
        _callbackToken = 0;
    }

    /// <inheritdoc />
    protected override void ReleaseUnmanagedResources()
    {
        // Freeing the native object stops its callback thread, so the token can go after it.
        RtMidiInterop.rtmidi_in_free(RtMidiPtr);
        _handler = null;
        UnregisterCallback();
    }
}
