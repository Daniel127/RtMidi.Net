using RtMidi.Net.Enums;
using System.Runtime.InteropServices;

namespace RtMidi.Net.InteropServices;

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
internal class RtMidiIn : RtMidiBase
{
#pragma warning disable IDE0052
    // ReSharper disable once NotAccessedField.Local
    // Used to prevent garbage collection of the delegate
    private RtMidiCallback? _midiCallback;
#pragma warning restore IDE0052

    /// <summary>
    /// Default constructor
    /// </summary>
    public RtMidiIn() : base(RtMidiInterop.rtmidi_in_create_default())
    {
    }

    /// <summary>
    /// <para>Default constructor that allows an optional api, client name and queue size.</para>
    /// <para>
    /// The queue size defines the maximum number of
    /// messages that can be held in the MIDI queue (when not using a
    /// callback function).  If the queue size limit is reached,
    /// incoming messages will be ignored.
    /// </para>
    /// <para>
    /// If no API argument is specified and multiple API support has been
    /// compiled, the default order of use is ALSA, JACK (Linux) and CORE,
    /// JACK (OS-X).
    /// </para>
    /// </summary>
    /// <remarks>An exception will be thrown if a MIDI system initialization
    /// error occurs.</remarks>
    /// <param name="api"></param>
    /// <param name="clientName"></param>
    /// <param name="queueSizeLimit"></param>
    public RtMidiIn(MidiApi api, string clientName, uint queueSizeLimit = 100)
        : base(RtMidiInterop.rtmidi_in_create(api, clientName, queueSizeLimit))
    {
    }

    /// <inheritdoc />
    public override MidiApi GetCurrentApi()
    {
        return RtMidiInterop.rtmidi_in_get_current_api(RtMidiPtr);
    }

    /// <summary>
    /// Set a callback function to be invoked for incoming MIDI messages.
    /// </summary>
    /// <remarks>
    /// The callback function will be called whenever an incoming MIDI
    /// message is received.  While not absolutely necessary, it is best
    /// to set the callback function before opening a MIDI port to avoid
    /// leaving some messages in the queue.
    /// </remarks>
    /// <param name="callback">A callback function must be given.</param>
    /// <param name="userData">Optionally, a pointer to additional data can be
    /// passed to the callback function whenever it is called.</param>
    public void SetCallback(RtMidiCallback callback, byte[]? userData)
    {
        _midiCallback = callback;
        RtMidiInterop.rtmidi_in_set_callback(RtMidiPtr, callback, userData);
    }

    /// <summary>
    /// Cancel use of the current callback function (if one exists).
    /// <para>
    /// Subsequent incoming MIDI messages will be written to the queue
    /// and can be retrieved with the <see cref="GetMessage"/> function.
    /// </para>
    /// </summary>
    public void CancelCallback()
    {
        RtMidiInterop.rtmidi_in_cancel_callback(RtMidiPtr);
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
    /// <param name="midiSysex">Ignore MIDI Sysex</param>
    /// <param name="midiTime">Ignore MIDI Time</param>
    /// <param name="midiSense">Ignore MIDI Sense</param>
    public void IgnoreTypes(bool midiSysex = true, bool midiTime = true, bool midiSense = true)
    {
        RtMidiInterop.rtmidi_in_ignore_types(RtMidiPtr, midiSysex, midiTime, midiSense);
    }

    /// <summary>
    /// Fill the user-provided vector with the data bytes
    /// for the next available MIDI message in the input queue
    /// and return the event delta-time in seconds. 
    /// </summary>
    /// <remarks>
    /// This function returns immediately whether a new message is
    /// available or not.
    /// </remarks>
    /// <returns>The message and timestamp</returns>
    public (byte[] message, double timestamp) GetMessage()
    {
        var size = 1024;
        var messagePtr = Marshal.AllocHGlobal(size);

        var timestamp = RtMidiInterop.rtmidi_in_get_message(RtMidiPtr, messagePtr, ref size);
        var message = new byte[size];
        Marshal.Copy(messagePtr, message, 0, size);
        Marshal.FreeHGlobal(messagePtr);

        return (message, timestamp);
    }

    /// <inheritdoc />
    protected override void ReleaseUnmanagedResources()
    {
        _midiCallback = null;
        RtMidiInterop.rtmidi_in_free(RtMidiPtr);
    }
}