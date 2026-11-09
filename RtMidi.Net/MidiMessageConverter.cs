using RtMidi.Net.Enums;

namespace RtMidi.Net;

/// <summary>
/// Converts between raw MIDI bytes and <see cref="MidiMessage"/> records.
/// </summary>
internal static class MidiMessageConverter
{
    private const byte StatusBit = 0b_1000_0000;
    private const byte DataMask = 0b_0111_1111;
    private const byte ChannelMask = 0b_0000_1111;
    private const byte ChannelTypeMask = 0b_1111_0000;

    /// <summary>
    /// Decodes a complete MIDI message (as delivered by RtMidi: status byte first, no running status).
    /// </summary>
    /// <remarks>
    /// Never throws: anything that is not a well-formed message (empty, missing data
    /// bytes, data bytes with the high bit set) becomes a <see cref="MidiMessageUnknown"/>.
    /// </remarks>
    public static MidiMessage Parse(ReadOnlySpan<byte> message)
    {
        if (message.IsEmpty || message[0] < StatusBit) return Unknown(message);

        var status = message[0];
        if (status < (byte)MidiMessageType.SystemExclusive)
        {
            return ParseChannelMessage(message);
        }

        var type = (MidiMessageType)status;
        if (type == MidiMessageType.SystemExclusive)
        {
            return new MidiMessageSystemExclusive(message.ToArray());
        }

        if (!HasValidData(message, DataLength(type))) return Unknown(message);

        return type switch
        {
            MidiMessageType.TimeCodeQuarterFrame => new MidiMessageTimeCodeQuarterFrame(
                (byte)((message[1] >> 4) & 0b_0111), (byte)(message[1] & 0b_1111)),
            MidiMessageType.SongPositionPointer => new MidiMessageSongPositionPointer(message[1], message[2]),
            MidiMessageType.SongSelect => new MidiMessageSongSelect(message[1]),
            _ => new MidiMessage(type),
        };
    }

    private static MidiMessage ParseChannelMessage(ReadOnlySpan<byte> message)
    {
        var type = (MidiMessageType)(message[0] & ChannelTypeMask);
        if (!HasValidData(message, DataLength(type))) return Unknown(message);

        var channel = (MidiChannel)(message[0] & ChannelMask);
        return type switch
        {
            MidiMessageType.NoteOff or MidiMessageType.NoteOn =>
                new MidiMessageNote(channel, new MidiNote(message[1]), message[2], type),
            MidiMessageType.PolyphonicKeyPressure =>
                new MidiMessageNoteAfterTouch(channel, new MidiNote(message[1]), message[2]),
            MidiMessageType.ControlChange => new MidiMessageControlChange(channel, message[1], message[2]),
            MidiMessageType.ProgramChange => new MidiMessageProgramChange(channel, message[1]),
            MidiMessageType.ChannelPressure => new MidiMessageChannelAfterTouch(channel, message[1]),
            // Only PitchBendChange is left in 0x80-0xEF.
            _ => new MidiMessagePitchBendChange(channel, message[1], message[2]),
        };
    }

    /// <summary>
    /// Number of data bytes that follow the status byte (SysEx excluded).
    /// </summary>
    internal static int DataLength(MidiMessageType type) => type switch
    {
        MidiMessageType.NoteOff or MidiMessageType.NoteOn or MidiMessageType.PolyphonicKeyPressure
            or MidiMessageType.ControlChange or MidiMessageType.PitchBendChange
            or MidiMessageType.SongPositionPointer => 2,
        MidiMessageType.ProgramChange or MidiMessageType.ChannelPressure
            or MidiMessageType.TimeCodeQuarterFrame or MidiMessageType.SongSelect => 1,
        _ => 0,
    };

    private static bool HasValidData(ReadOnlySpan<byte> message, int dataLength)
    {
        if (message.Length < dataLength + 1) return false;
        foreach (var dataByte in message.Slice(1, dataLength))
        {
            if (dataByte > DataMask) return false;
        }

        return true;
    }

    private static MidiMessageUnknown Unknown(ReadOnlySpan<byte> message) => new(message.ToArray());

    /// <summary>
    /// Encodes a message to the bytes sent to the MIDI port.
    /// </summary>
    /// <exception cref="ArgumentException">The record does not match its <see cref="MidiMessage.Type"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A value does not fit in its MIDI field (e.g. a data byte above 127).</exception>
    public static byte[] ToBytes(MidiMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return message switch
        {
            MidiMessageUnknown unknown => unknown.MessageData.ToArray(),
            MidiMessageNote { Type: MidiMessageType.NoteOff or MidiMessageType.NoteOn } note =>
                [Status(note.Type, note.Channel), Data(note.Note.Number), Data(note.Velocity)],
            MidiMessageNoteAfterTouch { Type: MidiMessageType.PolyphonicKeyPressure } afterTouch =>
                [Status(afterTouch.Type, afterTouch.Channel), Data(afterTouch.Note.Number), Data(afterTouch.Pressure)],
            MidiMessageControlChange { Type: MidiMessageType.ControlChange } controlChange =>
                [Status(controlChange.Type, controlChange.Channel), Data(controlChange.ControlFunction), Data(controlChange.Value)],
            MidiMessageProgramChange { Type: MidiMessageType.ProgramChange } programChange =>
                [Status(programChange.Type, programChange.Channel), Data(programChange.Program)],
            MidiMessageChannelAfterTouch { Type: MidiMessageType.ChannelPressure } channelPressure =>
                [Status(channelPressure.Type, channelPressure.Channel), Data(channelPressure.Pressure)],
            MidiMessagePitchBendChange { Type: MidiMessageType.PitchBendChange } pitchBend =>
                [Status(pitchBend.Type, pitchBend.Channel), Data(pitchBend.Lsb), Data(pitchBend.Msb)],
            MidiMessageSystemExclusive { Type: MidiMessageType.SystemExclusive } sysEx => sysEx.Data.ToArray(),
            MidiMessageTimeCodeQuarterFrame { Type: MidiMessageType.TimeCodeQuarterFrame } quarterFrame =>
                [(byte)quarterFrame.Type, QuarterFrameData(quarterFrame)],
            MidiMessageSongPositionPointer { Type: MidiMessageType.SongPositionPointer } songPosition =>
                [(byte)songPosition.Type, Data(songPosition.Lsb), Data(songPosition.Msb)],
            MidiMessageSongSelect { Type: MidiMessageType.SongSelect } songSelect =>
                [(byte)songSelect.Type, Data(songSelect.SongNumber)],
            // Only the base record: a derived one (e.g. a note) with a single-byte type is a mismatch.
            _ when message.GetType() == typeof(MidiMessage) && IsSingleByteMessage(message.Type) => [(byte)message.Type],
            _ => throw new ArgumentException(
                $"A {message.GetType().Name} with type {message.Type} can't be encoded.", nameof(message)),
        };
    }

    private static bool IsSingleByteMessage(MidiMessageType type) =>
        type > MidiMessageType.SongSelect;

    private static byte Status(MidiMessageType type, MidiChannel channel)
    {
        if ((byte)channel > ChannelMask)
            throw new ArgumentOutOfRangeException(nameof(channel), channel, "The channel must be 0 to 15.");
        return (byte)((byte)type | (byte)channel);
    }

    private static byte Data(byte value)
    {
        if (value > DataMask)
            throw new ArgumentOutOfRangeException(nameof(value), value, "MIDI data bytes must be 0 to 127.");
        return value;
    }

    private static byte QuarterFrameData(MidiMessageTimeCodeQuarterFrame quarterFrame)
    {
        if (quarterFrame.Piece > 0b_0111)
            throw new ArgumentOutOfRangeException(nameof(quarterFrame), quarterFrame.Piece, "The quarter frame piece must be 0 to 7.");
        if (quarterFrame.Value > 0b_1111)
            throw new ArgumentOutOfRangeException(nameof(quarterFrame), quarterFrame.Value, "The quarter frame value must be 0 to 15.");
        return (byte)((quarterFrame.Piece << 4) | quarterFrame.Value);
    }
}
