using RtMidi.Net.Enums;

namespace RtMidi.Net.InteropServices;

/// <summary>
/// This class provides a common, platform-independent API for MIDI
/// output.  It allows one to probe available MIDI output ports, to
/// connect to one such port, and to send MIDI bytes immediately over
/// the connection.  Create multiple instances of this class to
/// connect to more than one MIDI device at the same time.  With the
/// OS-X, Linux ALSA and JACK MIDI APIs, it is also possible to open a
/// virtual port to which other MIDI software clients can connect.
/// </summary>
internal class RtMidiOut : RtMidiBase
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public RtMidiOut() : base(RtMidiInterop.rtmidi_out_create_default())
    {
    }

    /// <summary>
    /// Default constructor that allows an optional client name.
    /// <remarks>
    /// An exception will be thrown if a MIDI system initialization error occurs.
    /// 
    /// If no API argument is specified and multiple API support has been
    /// compiled, the default order of use is ALSA, JACK (Linux) and CORE,
    /// JACK (OS-X).
    /// </remarks>
    /// </summary>
    /// <param name="api">The api</param>
    /// <param name="clientName">The client name</param>
    public RtMidiOut(MidiApi api, string clientName)
        : base(RtMidiInterop.rtmidi_out_create(api, clientName))
    {
    }

    /// <inheritdoc />
    public override MidiApi GetCurrentApi()
    {
        return RtMidiInterop.rtmidi_out_get_current_api(RtMidiPtr);
    }

    /// <summary>
    /// Immediately send a single message out an open MIDI output port.
    /// </summary>
    /// <remarks>An exception is thrown if an error occurs during output or an
    /// output connection was not previously established.</remarks>
    /// <param name="message">Message to send</param>
    public void SendMessage(byte[] message)
    {
        var result = RtMidiInterop.rtmidi_out_send_message(RtMidiPtr, message, message.Length);
        if (result == -1) throw new Exception();
    }

    /// <inheritdoc />
    protected override void ReleaseUnmanagedResources()
    {
        RtMidiInterop.rtmidi_out_free(RtMidiPtr);
    }
}