namespace RtMidi.Net.Enums;

/// <summary>
/// MIDI backend used by RtMidi.
/// </summary>
public enum MidiApi : byte
{
    /// <summary>Let RtMidi choose a compiled API.</summary>
    Unspecified,
    /// <summary>macOS CoreMIDI.</summary>
    MacOsxCore,
    /// <summary>Linux ALSA sequencer.</summary>
    LinuxAlsa,
    /// <summary>JACK.</summary>
    UnixJack,
    /// <summary>Windows MultiMedia (WinMM).</summary>
    WindowsMultimediaMidi,
    /// <summary>Dummy API that does nothing (no other API compiled).</summary>
    RtMidiDummy,
    /// <summary>W3C Web MIDI API.</summary>
    WebMidi,
    /// <summary>Windows UWP MIDI API.</summary>
    WindowsUwp,
    /// <summary>Android MIDI API.</summary>
    Android,
    /// <summary>Windows MIDI Services (Windows 11 25H2 and later; RtMidi 6.1).</summary>
    WindowsMidiServices,
}
