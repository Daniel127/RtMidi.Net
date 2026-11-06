using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Clients;

/// <summary>
/// Sends MIDI messages to an output port.
/// </summary>
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
    public MidiOutputClient(MidiDeviceInfo deviceInfo) : base(deviceInfo, new RtMidiOut())
    {
        _rtMidiOutClient = (RtMidiOut)RtMidiClient;
    }

    /// <summary>
    /// Sends raw MIDI bytes.
    /// </summary>
    public void SendMessage(byte[] message)
    {
        ArgumentNullException.ThrowIfNull(message);
        _rtMidiOutClient.SendMessage(message);
    }

    /// <summary>
    /// Encodes and sends a message.
    /// </summary>
    /// <exception cref="ArgumentException">The message can't be encoded (its record does not match its type).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A value does not fit in its MIDI field (e.g. a data byte above 127).</exception>
    public void SendMessage(MidiMessage message)
    {
        _rtMidiOutClient.SendMessage(MidiMessageConverter.ToBytes(message));
    }
}
