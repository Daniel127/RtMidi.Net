namespace RtMidi.Net.Enums;

public enum MidiApi : byte
{
    Unspecified,
    MacOsxCore,
    LinuxAlsa,
    UnixJack,
    WindowsMultimediaMidi,
    RtMidiDummy,
    WebMidi,
    WindowsUwp,
    Android,
}