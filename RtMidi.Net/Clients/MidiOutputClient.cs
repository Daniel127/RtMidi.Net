using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Clients;

/// <summary>
/// Sends MIDI messages to an output port.
/// </summary>
/// <example>
/// <code>
/// using var output = new MidiOutputClient(device);
/// output.Open();
/// var middleC = MidiNote.Parse("C4");
/// output.SendMessage(new MidiMessageNote(MidiChannel.Channel01, middleC, 100, MidiMessageType.NoteOn));
/// output.SendMessage(new MidiMessageNote(MidiChannel.Channel01, middleC, 0, MidiMessageType.NoteOff));
/// </code>
/// </example>
public class MidiOutputClient : MidiClient
{
    private readonly RtMidiOut _rtMidiOutClient;

    /// <summary>
    /// Creates a client that is not bound to a device, to be used with
    /// <see cref="MidiClient.OpenVirtual"/>.
    /// </summary>
    public MidiOutputClient() : base(new RtMidiOut())
    {
        _rtMidiOutClient = (RtMidiOut)RtMidiClient;
    }

    /// <summary>
    /// Creates a client for the given output device. Call <see cref="MidiClient.Open"/> to connect.
    /// </summary>
    /// <param name="deviceInfo">An output port from <see cref="MidiManager"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="deviceInfo"/> is null.</exception>
    public MidiOutputClient(MidiDeviceInfo deviceInfo) : base(deviceInfo, new RtMidiOut())
    {
        _rtMidiOutClient = (RtMidiOut)RtMidiClient;
    }

    /// <summary>
    /// Sends raw MIDI bytes, without checking them.
    /// </summary>
    /// <param name="message">One complete MIDI message, status byte first.</param>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is empty.</exception>
    /// <exception cref="RtMidiException">RtMidi reported an error while sending.</exception>
    public void SendMessage(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _rtMidiOutClient.SendMessage(message);
    }

    /// <summary>
    /// Encodes and sends a message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
    /// <exception cref="RtMidiException">RtMidi reported an error while sending.</exception>
    /// <exception cref="ArgumentException">The message can't be encoded (its record does not match its type).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A value does not fit in its MIDI field (e.g. a data byte above 127).</exception>
    public void SendMessage(MidiMessage message)
    {
        _rtMidiOutClient.SendMessage(MidiMessageConverter.ToBytes(message));
    }
}
