using RtMidi.Net.Enums;

namespace RtMidi.Net;

public record MidiMessage(MidiMessageType Type);
public record MidiMessageUnknown(byte[] MessageData, MidiMessageType Type = MidiMessageType.Unknown) : MidiMessage(Type);
public abstract record MidiMessageNoteBase(MidiChannel Channel, MidiNote Note, MidiMessageType Type) : MidiMessage(Type);
public record MidiMessageNote(MidiChannel Channel, MidiNote Note, byte Velocity, MidiMessageType Type) : MidiMessageNoteBase(Channel, Note, Type);
public record MidiMessageNoteAfterTouch(MidiChannel Channel, MidiNote Note, byte Pressure, MidiMessageType Type = MidiMessageType.PolyphonicKeyPressure) : MidiMessageNoteBase(Channel, Note, Type);
//TODO Create enum for ControlFunction
public record MidiMessageControlChange(MidiChannel Channel, byte ControlFunction, byte Value, MidiMessageType Type = MidiMessageType.ControlChange) : MidiMessage(Type);
public record MidiMessageProgramChange(MidiChannel Channel, byte Program, MidiMessageType Type = MidiMessageType.ProgramChange) : MidiMessage(Type);
public record MidiMessageChannelAfterTouch(MidiChannel Channel, byte Pressure, MidiMessageType Type = MidiMessageType.ChannelPressure) : MidiMessage(Type);
public record MidiMessagePitchBendChange(MidiChannel Chanel, byte Lsb, byte Msb, MidiMessageType Type = MidiMessageType.PitchBendChange) : MidiMessage(Type);
//TODO Create Manufacturer enum?; Create SysEx type?
//See https://www.recordingblogs.com/wiki/midi-system-exclusive-message
public record MidiMessageSystemExclusive(byte[] Data, MidiMessageType Type = MidiMessageType.SystemExclusive) : MidiMessage(Type);
public record MidiMessageSongPositionPointer(byte Lsb, byte Msb, MidiMessageType Type = MidiMessageType.SongPositionPointer) : MidiMessage(Type);
public record MidiMessageSongSelect(byte SongNumber, MidiMessageType Type = MidiMessageType.SongSelect) : MidiMessage(Type);

//TODO Create Message for TimeCodeQuarterFrame