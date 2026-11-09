
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
internal sealed class RtMidiOut : RtMidiBase
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public RtMidiOut() : base(EnsureCreated(RtMidiInterop.rtmidi_out_create_default(), RtMidiInterop.rtmidi_out_free))
    {
    }

    /// <summary>
    /// Constructor that allows an optional api and client name.
    /// </summary>
    /// <remarks>
    /// An exception will be thrown if a MIDI system initialization error occurs.
    /// </remarks>
    /// <param name="api">The api</param>
    /// <param name="clientName">The client name</param>
    public RtMidiOut(MidiApi api, string clientName)
        : base(EnsureCreated(RtMidiInterop.rtmidi_out_create((int)api, clientName), RtMidiInterop.rtmidi_out_free))
    {
    }

    /// <inheritdoc />
    protected override string DefaultPortName => "RtMidi Output";

    /// <inheritdoc />
    public override MidiApi GetCurrentApi()
    {
        return (MidiApi)RtMidiInterop.rtmidi_out_get_current_api(RtMidiPtr);
    }

    /// <summary>
    /// Immediately send a single message out an open MIDI output port.
    /// </summary>
    /// <remarks>An exception is thrown if an error occurs during output or an
    /// output connection was not previously established.</remarks>
    /// <param name="message">Message to send</param>
    public void SendMessage(ReadOnlySpan<byte> message)
    {
        if (message.IsEmpty) throw new ArgumentException("The message is empty.", nameof(message));

        RtMidiInterop.rtmidi_out_send_message(RtMidiPtr, message, message.Length);
        ThrowIfError();
    }

    /// <inheritdoc />
    protected override void ReleaseUnmanagedResources(IntPtr rtMidiPtr)
    {
        RtMidiInterop.rtmidi_out_free(rtMidiPtr);
    }
}
