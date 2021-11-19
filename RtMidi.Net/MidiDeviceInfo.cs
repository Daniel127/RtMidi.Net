using RtMidi.Net.Enums;

namespace RtMidi.Net;

public class MidiDeviceInfo
{
    public string Name { get; }
    public uint Port { get; }
    public MidiApi Api { get; }
    public MidiDeviceType Type { get; }

    internal MidiDeviceInfo(string name, uint port, MidiApi api, MidiDeviceType type)
    {
        Name = name;
        Port = port;
        Api = api;
        Type = type;
    }

    public void Deconstruct(out string name, out uint port, out MidiApi api, out MidiDeviceType type)
    {
        name = Name;
        port = Port;
        api = Api;
        type = Type;
    }
}