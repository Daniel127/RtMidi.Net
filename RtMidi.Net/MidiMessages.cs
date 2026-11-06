using RtMidi.Net.Enums;

namespace RtMidi.Net;

/// <summary>
/// A MIDI message. Messages without data (e.g. <see cref="MidiMessageType.TimingClock"/>) use this type directly.
/// </summary>
/// <param name="Type">The message type</param>
public record MidiMessage(MidiMessageType Type);

/// <summary>
/// A message that could not be decoded; <paramref name="MessageData"/> holds the raw bytes.
/// </summary>
public record MidiMessageUnknown(byte[] MessageData, MidiMessageType Type = MidiMessageType.Unknown) : MidiMessage(Type);

/// <summary>
/// Base of the channel messages that refer to a note.
/// </summary>
public abstract record MidiMessageNoteBase(MidiChannel Channel, MidiNote Note, MidiMessageType Type) : MidiMessage(Type);

/// <summary>
/// Note On / Note Off.
/// </summary>
public record MidiMessageNote(MidiChannel Channel, MidiNote Note, byte Velocity, MidiMessageType Type) : MidiMessageNoteBase(Channel, Note, Type);

/// <summary>
/// Polyphonic Key Pressure (aftertouch of a single note).
/// </summary>
public record MidiMessageNoteAfterTouch(MidiChannel Channel, MidiNote Note, byte Pressure, MidiMessageType Type = MidiMessageType.PolyphonicKeyPressure) : MidiMessageNoteBase(Channel, Note, Type);

/// <summary>
/// Control Change. <paramref name="ControlFunction"/> is the controller number (0-127).
/// </summary>
public record MidiMessageControlChange(MidiChannel Channel, byte ControlFunction, byte Value, MidiMessageType Type = MidiMessageType.ControlChange) : MidiMessage(Type);

/// <summary>
/// Program Change.
/// </summary>
public record MidiMessageProgramChange(MidiChannel Channel, byte Program, MidiMessageType Type = MidiMessageType.ProgramChange) : MidiMessage(Type);

/// <summary>
/// Channel Pressure (aftertouch of the whole channel).
/// </summary>
public record MidiMessageChannelAfterTouch(MidiChannel Channel, byte Pressure, MidiMessageType Type = MidiMessageType.ChannelPressure) : MidiMessage(Type);

/// <summary>
/// Pitch Bend Change, a 14-bit value split in two 7-bit bytes.
/// </summary>
public record MidiMessagePitchBendChange(MidiChannel Channel, byte Lsb, byte Msb, MidiMessageType Type = MidiMessageType.PitchBendChange) : MidiMessage(Type)
{
    /// <summary>
    /// The channel. Kept for compatibility, use <see cref="Channel"/>.
    /// </summary>
    [Obsolete("Use Channel instead.")]
    public MidiChannel Chanel
    {
        get => Channel;
        init => Channel = value;
    }

    /// <summary>
    /// The 14-bit bend value: 0 to 16383, 8192 is the centre (no bend).
    /// </summary>
    public int Value => ((Msb & 0x7F) << 7) | (Lsb & 0x7F);
}

/// <summary>
/// System Exclusive. <paramref name="Data"/> is the whole message, from 0xF0 to 0xF7.
/// See https://www.recordingblogs.com/wiki/midi-system-exclusive-message
/// </summary>
public record MidiMessageSystemExclusive(byte[] Data, MidiMessageType Type = MidiMessageType.SystemExclusive) : MidiMessage(Type);

/// <summary>
/// MIDI Time Code Quarter Frame.
/// </summary>
/// <param name="Piece">Which part of the time code this message carries (0-7):
/// 0/1 frames, 2/3 seconds, 4/5 minutes, 6/7 hours and rate.</param>
/// <param name="Value">The 4-bit value of that part (0-15).</param>
/// <param name="Type">The message type</param>
public record MidiMessageTimeCodeQuarterFrame(byte Piece, byte Value, MidiMessageType Type = MidiMessageType.TimeCodeQuarterFrame) : MidiMessage(Type);

/// <summary>
/// Song Position Pointer, a 14-bit value (in MIDI beats) split in two 7-bit bytes.
/// </summary>
public record MidiMessageSongPositionPointer(byte Lsb, byte Msb, MidiMessageType Type = MidiMessageType.SongPositionPointer) : MidiMessage(Type)
{
    /// <summary>
    /// The 14-bit position, in MIDI beats (sixteenth notes) since the start of the song.
    /// </summary>
    public int Position => ((Msb & 0x7F) << 7) | (Lsb & 0x7F);
}

/// <summary>
/// Song Select.
/// </summary>
public record MidiMessageSongSelect(byte SongNumber, MidiMessageType Type = MidiMessageType.SongSelect) : MidiMessage(Type);
