using RtMidi.Net.Enums;

namespace RtMidi.Net;

/// <summary>
/// A MIDI port found by <see cref="MidiManager"/>.
/// </summary>
public class MidiDeviceInfo
{
    /// <summary>
    /// Port name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Port number, used to open it.
    /// </summary>
    public uint Port { get; }

    /// <summary>
    /// MIDI API that found the port.
    /// </summary>
    public MidiApi Api { get; }

    /// <summary>
    /// Input or output.
    /// </summary>
    public MidiDeviceType Type { get; }

    internal MidiDeviceInfo(string name, uint port, MidiApi api, MidiDeviceType type)
    {
        Name = name;
        Port = port;
        Api = api;
        Type = type;
    }

    /// <summary>
    /// Deconstructs the device into its properties.
    /// </summary>
    public void Deconstruct(out string name, out uint port, out MidiApi api, out MidiDeviceType type)
    {
        name = Name;
        port = Port;
        api = Api;
        type = Type;
    }
}