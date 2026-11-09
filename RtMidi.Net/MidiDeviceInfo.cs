using RtMidi.Net.Enums;

namespace RtMidi.Net;

/// <summary>
/// A MIDI port found by <see cref="MidiManager"/>. Pass it to <see cref="Clients.MidiInputClient(MidiDeviceInfo)"/>
/// or <see cref="Clients.MidiOutputClient(MidiDeviceInfo)"/> to open it.
/// </summary>
/// <remarks>
/// It is a snapshot: port numbers can change when devices are connected or disconnected, so look the
/// devices up again after a change.
/// </remarks>
public class MidiDeviceInfo
{
    /// <summary>
    /// Port name, as given by the operating system.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Port number for <see cref="Api"/> and <see cref="Type"/>, used to open it.
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
    /// <param name="name">The <see cref="Name"/>.</param>
    /// <param name="port">The <see cref="Port"/>.</param>
    /// <param name="api">The <see cref="Api"/>.</param>
    /// <param name="type">The <see cref="Type"/>.</param>
    public void Deconstruct(out string name, out uint port, out MidiApi api, out MidiDeviceType type)
    {
        name = Name;
        port = Port;
        api = Api;
        type = Type;
    }
}