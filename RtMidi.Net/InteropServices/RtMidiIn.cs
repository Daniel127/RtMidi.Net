using RtMidi.Net.Enums;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RtMidi.Net.InteropServices;

/// <summary>
/// Handler for messages received through the RtMidi callback.
/// </summary>
/// <param name="timestamp">Delta-time in seconds since the previous message.</param>
/// <param name="message">The message bytes, only valid during the call.</param>
internal delegate void RtMidiMessageHandler(double timestamp, ReadOnlySpan<byte> message);

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
internal sealed unsafe class RtMidiIn : RtMidiBase
{
    /// <summary>
    /// Size of the buffer used by <see cref="GetMessage"/>. RtMidi drops a queued
    /// message that does not fit, so it is large enough for long SysEx dumps.
    /// </summary>
    internal const int MessageBufferSize = 64 * 1024;

    private RtMidiMessageHandler? _handler;
    private GCHandle _callbackHandle;
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
        if (!_callbackHandle.IsAllocated)
        {
            _callbackHandle = GCHandle.Alloc(this);
        }

        RtMidiInterop.rtmidi_in_set_callback(RtMidiPtr, &OnNativeMessage, GCHandle.ToIntPtr(_callbackHandle));
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
        _handler = null;
        FreeCallbackHandle();
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
            double timestamp;
            fixed (byte* bufferPtr = _messageBuffer)
            {
                timestamp = RtMidiInterop.rtmidi_in_get_message(RtMidiPtr, bufferPtr, &size);
            }

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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnNativeMessage(double timestamp, byte* message, nuint size, IntPtr userData)
    {
        // An exception must never cross back into native code (it would terminate the process).
        try
        {
            if (userData == IntPtr.Zero) return;
            if (GCHandle.FromIntPtr(userData).Target is not RtMidiIn { _handler: { } handler }) return;
            handler(timestamp, new ReadOnlySpan<byte>(message, checked((int)size)));
        }
        catch (Exception ex)
        {
            Trace.TraceError($"RtMidi.Net: unhandled exception in the MIDI message handler: {ex}");
        }
    }

    /// <summary>
    /// The native callback and the user data RtMidi passes to it (for tests).
    /// </summary>
    internal static delegate* unmanaged[Cdecl]<double, byte*, nuint, IntPtr, void> NativeCallback => &OnNativeMessage;

    internal IntPtr CallbackUserData => _callbackHandle.IsAllocated ? GCHandle.ToIntPtr(_callbackHandle) : IntPtr.Zero;

    private void FreeCallbackHandle()
    {
        if (_callbackHandle.IsAllocated) _callbackHandle.Free();
    }

    /// <inheritdoc />
    protected override void ReleaseUnmanagedResources()
    {
        // Freeing the native object stops its callback thread, so the handle can go after it.
        RtMidiInterop.rtmidi_in_free(RtMidiPtr);
        _handler = null;
        FreeCallbackHandle();
    }
}
