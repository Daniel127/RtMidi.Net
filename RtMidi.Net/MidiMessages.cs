
namespace RtMidi.Net;

/// <summary>
/// A MIDI message. Messages without data (e.g. <see cref="MidiMessageType.TimingClock"/>) use this type directly;
/// the others use a derived record, so a received message is usually examined with pattern matching.
/// </summary>
/// <remarks>
/// Messages are records: two messages with the same values are equal. Byte array properties
/// (<see cref="MidiMessageUnknown.MessageData"/>, <see cref="MidiMessageSystemExclusive.Data"/>) are compared by
/// reference, not by content.
/// </remarks>
/// <example>
/// <code>
/// switch (message)
/// {
///     case MidiMessageNote { Type: MidiMessageType.NoteOn, Velocity: > 0 } noteOn:
///         Console.WriteLine($"{noteOn.Note} pressed with velocity {noteOn.Velocity}");
///         break;
///     case MidiMessageControlChange controlChange:
///         Console.WriteLine($"CC {controlChange.ControlFunction} = {controlChange.Value}");
///         break;
/// }
/// </code>
/// </example>
/// <param name="Type">The message type. For channel messages it is the status byte without the channel.</param>
public record MidiMessage(MidiMessageType Type);

/// <summary>
/// A message that could not be decoded (empty, unknown status byte, missing data bytes or data bytes above 127).
/// Sending it sends <paramref name="MessageData"/> unchanged.
/// </summary>
/// <param name="MessageData">The raw bytes of the message, as received.</param>
public record MidiMessageUnknown(byte[] MessageData) : MidiMessage(MidiMessageType.Unknown);

/// <summary>
/// Base of the channel messages that refer to a note: <see cref="MidiMessageNote"/> and <see cref="MidiMessageNoteAfterTouch"/>.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Note">The note. <see cref="MidiNote.ToString"/> gives its name in scientific pitch notation (middle C is C4).</param>
/// <param name="Type">The message type.</param>
public abstract record MidiMessageNoteBase(MidiChannel Channel, MidiNote Note, MidiMessageType Type) : MidiMessage(Type);

/// <summary>
/// Note On or Note Off, depending on <see cref="MidiMessage.Type"/>.
/// </summary>
/// <remarks>
/// Many devices send a Note On with velocity 0 instead of a Note Off. Messages are delivered as received,
/// so check both when looking for released keys.
/// </remarks>
/// <example>
/// <code>
/// output.SendMessage(new MidiMessageNote(MidiChannel.Channel01, MidiNote.Parse("C4"), 100, MidiMessageType.NoteOn));
/// </code>
/// </example>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Note">The note (MIDI note 60 is middle C, C4).</param>
/// <param name="Velocity">How hard the key was pressed or released, 0 to 127. A Note On with 0 means Note Off.</param>
/// <param name="Type"><see cref="MidiMessageType.NoteOn"/> or <see cref="MidiMessageType.NoteOff"/>; any other type
/// can't be sent.</param>
public record MidiMessageNote(MidiChannel Channel, MidiNote Note, byte Velocity, MidiMessageType Type) : MidiMessageNoteBase(Channel, Note, Type);

/// <summary>
/// Polyphonic Key Pressure: the aftertouch of a single held note.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Note">The held note.</param>
/// <param name="Pressure">The pressure on the key, 0 to 127.</param>
public record MidiMessageNoteAfterTouch(MidiChannel Channel, MidiNote Note, byte Pressure) : MidiMessageNoteBase(Channel, Note, MidiMessageType.PolyphonicKeyPressure);

/// <summary>
/// Control Change: a controller (knob, fader, pedal...) moved, or a channel mode message (controllers 120 to 127).
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="ControlFunction">The controller number, 0 to 127 (e.g. 1 modulation wheel, 7 volume, 64 sustain pedal).</param>
/// <param name="Value">The controller value, 0 to 127.</param>
public record MidiMessageControlChange(MidiChannel Channel, byte ControlFunction, byte Value) : MidiMessage(MidiMessageType.ControlChange);

/// <summary>
/// Program Change: selects an instrument or preset.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Program">The program number, 0 to 127 (shown as 1 to 128 by many devices).</param>
public record MidiMessageProgramChange(MidiChannel Channel, byte Program) : MidiMessage(MidiMessageType.ProgramChange);

/// <summary>
/// Channel Pressure: a single aftertouch value for the whole channel, for devices without per-key pressure.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Pressure">The pressure, 0 to 127.</param>
public record MidiMessageChannelAfterTouch(MidiChannel Channel, byte Pressure) : MidiMessage(MidiMessageType.ChannelPressure);

/// <summary>
/// Pitch Bend Change, a 14-bit value split in two 7-bit bytes. Use <see cref="Value"/> for the combined value.
/// </summary>
/// <param name="Channel">The MIDI channel.</param>
/// <param name="Lsb">The low 7 bits of the bend value, 0 to 127.</param>
/// <param name="Msb">The high 7 bits of the bend value, 0 to 127.</param>
public record MidiMessagePitchBendChange(MidiChannel Channel, byte Lsb, byte Msb) : MidiMessage(MidiMessageType.PitchBendChange)
{
    /// <summary>
    /// The 14-bit bend value: 0 to 16383, 8192 is the centre (no bend).
    /// </summary>
    public int Value => ((Msb & 0x7F) << 7) | (Lsb & 0x7F);
}

/// <summary>
/// System Exclusive (SysEx): manufacturer-specific data. Only received when
/// <see cref="MidiInputClient.IgnoreSysexMessages"/> is <see langword="false"/>.
/// See https://www.recordingblogs.com/wiki/midi-system-exclusive-message
/// </summary>
/// <param name="Data">The whole message, from 0xF0 to 0xF7 included. It is sent as is.</param>
public record MidiMessageSystemExclusive(byte[] Data) : MidiMessage(MidiMessageType.SystemExclusive);

/// <summary>
/// MIDI Time Code Quarter Frame. Eight of them, one per <paramref name="Piece"/>, make a complete time code.
/// Only received when <see cref="MidiInputClient.IgnoreTimeMessages"/> is <see langword="false"/>.
/// </summary>
/// <param name="Piece">Which part of the time code this message carries (0-7):
/// 0/1 frames, 2/3 seconds, 4/5 minutes, 6/7 hours and rate.</param>
/// <param name="Value">The 4-bit value of that part (0-15).</param>
public record MidiMessageTimeCodeQuarterFrame(byte Piece, byte Value) : MidiMessage(MidiMessageType.TimeCodeQuarterFrame);

/// <summary>
/// Song Position Pointer, a 14-bit value (in MIDI beats) split in two 7-bit bytes. Use <see cref="Position"/>
/// for the combined value.
/// </summary>
/// <param name="Lsb">The low 7 bits of the position, 0 to 127.</param>
/// <param name="Msb">The high 7 bits of the position, 0 to 127.</param>
public record MidiMessageSongPositionPointer(byte Lsb, byte Msb) : MidiMessage(MidiMessageType.SongPositionPointer)
{
    /// <summary>
    /// The 14-bit position, in MIDI beats (sixteenth notes) since the start of the song.
    /// </summary>
    public int Position => ((Msb & 0x7F) << 7) | (Lsb & 0x7F);
}

/// <summary>
/// Song Select: chooses the song or sequence to play.
/// </summary>
/// <param name="SongNumber">The song number, 0 to 127.</param>
public record MidiMessageSongSelect(byte SongNumber) : MidiMessage(MidiMessageType.SongSelect);
