namespace RtMidi.Net;

/// <summary>
/// The frame rate (SMPTE type) of a MIDI Time Code, as encoded in the hours field of the Quarter Frame and Full
/// messages (MIDI Time Code specification).
/// </summary>
public enum MidiTimeCodeRate : byte
{
    /// <summary>24 frames per second (film).</summary>
    Fps24 = 0,
    /// <summary>25 frames per second (PAL video).</summary>
    Fps25 = 1,
    /// <summary>
    /// 30 frames per second drop-frame (29.97 fps NTSC video): frame numbers 0 and 1 are skipped at the start of
    /// every minute except every tenth minute.
    /// </summary>
    Fps30DropFrame = 2,
    /// <summary>30 frames per second, non-drop.</summary>
    Fps30 = 3,
}
