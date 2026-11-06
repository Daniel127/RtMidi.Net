using RtMidi.Net.Enums;

namespace RtMidi.Net.Tests;

public class MidiMessageConverterToBytesTests
{
    private static readonly MidiNote MiddleC = new(60);

    public static TheoryData<MidiMessage, byte[]> Messages => new()
    {
        { new MidiMessageNote(MidiChannel.Channel01, MiddleC, 100, MidiMessageType.NoteOn), new byte[] { 0x90, 60, 100 } },
        { new MidiMessageNote(MidiChannel.Channel16, MiddleC, 0, MidiMessageType.NoteOff), new byte[] { 0x8F, 60, 0 } },
        { new MidiMessageNoteAfterTouch(MidiChannel.Channel02, new MidiNote(127), 10), new byte[] { 0xA1, 127, 10 } },
        { new MidiMessageControlChange(MidiChannel.Channel10, 64, 127), new byte[] { 0xB9, 64, 127 } },
        { new MidiMessageProgramChange(MidiChannel.Channel03, 5), new byte[] { 0xC2, 5 } },
        { new MidiMessageChannelAfterTouch(MidiChannel.Channel04, 90), new byte[] { 0xD3, 90 } },
        { new MidiMessagePitchBendChange(MidiChannel.Channel05, 0x7F, 0x7F), new byte[] { 0xE4, 0x7F, 0x7F } },
        { new MidiMessageSystemExclusive([0xF0, 0x43, 0x10, 0xF7]), new byte[] { 0xF0, 0x43, 0x10, 0xF7 } },
        { new MidiMessageTimeCodeQuarterFrame(7, 15), new byte[] { 0xF1, 0x7F } },
        { new MidiMessageTimeCodeQuarterFrame(2, 3), new byte[] { 0xF1, 0x23 } },
        { new MidiMessageSongPositionPointer(0x10, 0x20), new byte[] { 0xF2, 0x10, 0x20 } },
        { new MidiMessageSongSelect(42), new byte[] { 0xF3, 42 } },
        { new MidiMessage(MidiMessageType.TuneRequest), new byte[] { 0xF6 } },
        { new MidiMessage(MidiMessageType.EndOfSysEx), new byte[] { 0xF7 } },
        { new MidiMessage(MidiMessageType.TimingClock), new byte[] { 0xF8 } },
        { new MidiMessage(MidiMessageType.Start), new byte[] { 0xFA } },
        { new MidiMessage(MidiMessageType.Continue), new byte[] { 0xFB } },
        { new MidiMessage(MidiMessageType.Stop), new byte[] { 0xFC } },
        { new MidiMessage(MidiMessageType.ActiveSensing), new byte[] { 0xFE } },
        { new MidiMessage(MidiMessageType.SystemReset), new byte[] { 0xFF } },
        { new MidiMessage(MidiMessageType.UndefinedReserved1), new byte[] { 0xF4 } },
        { new MidiMessage(MidiMessageType.UndefinedReserved4), new byte[] { 0xFD } },
        { new MidiMessageUnknown([0x01, 0x02, 0x03]), new byte[] { 0x01, 0x02, 0x03 } },
        // Raw bytes with an explicit type (how Quarter Frames were sent before they had their own record).
        { new MidiMessageUnknown([0xF1, 0x23], MidiMessageType.TimeCodeQuarterFrame), new byte[] { 0xF1, 0x23 } },
    };

    [Theory]
    [MemberData(nameof(Messages))]
    public void ToBytes_EncodesMessage(MidiMessage message, byte[] expected)
    {
        Assert.Equal(expected, MidiMessageConverter.ToBytes(message));
    }

    [Theory]
    [MemberData(nameof(Messages))]
    public void Parse_OfToBytes_RoundTrips(MidiMessage message, byte[] bytes)
    {
        _ = bytes;
        if (message is MidiMessageUnknown) return; // raw bytes are decoded into the real message

        var roundTrip = MidiMessageConverter.Parse(MidiMessageConverter.ToBytes(message));

        if (message is MidiMessageSystemExclusive sysEx)
        {
            Assert.Equal(sysEx.Data, Assert.IsType<MidiMessageSystemExclusive>(roundTrip).Data);
        }
        else
        {
            Assert.Equal(message, roundTrip);
        }
    }

    [Fact]
    public void ToBytes_ReturnsCopyOfRawData()
    {
        byte[] data = [0xF0, 0x01, 0xF7];

        var bytes = MidiMessageConverter.ToBytes(new MidiMessageSystemExclusive(data));
        bytes[1] = 0x7F;

        Assert.Equal(0x01, data[1]);
    }

    [Fact]
    public void ToBytes_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MidiMessageConverter.ToBytes(null!));
    }

    public static TheoryData<MidiMessage> MismatchedMessages => new(new MidiMessage[]
    {
        new MidiMessage(MidiMessageType.NoteOn),
        new MidiMessage(MidiMessageType.SystemExclusive),
        new MidiMessage(MidiMessageType.Unknown),
        new MidiMessageNote(MidiChannel.Channel01, MiddleC, 10, MidiMessageType.ControlChange),
        new MidiMessageControlChange(MidiChannel.Channel01, 1, 1, MidiMessageType.NoteOn),
        new MidiMessageSongSelect(1, MidiMessageType.SongPositionPointer),
    });

    [Theory]
    [MemberData(nameof(MismatchedMessages))]
    public void ToBytes_RecordThatDoesNotMatchItsType_Throws(MidiMessage message)
    {
        var exception = Assert.Throws<ArgumentException>(() => MidiMessageConverter.ToBytes(message));
        Assert.Equal("message", exception.ParamName);
    }

    public static TheoryData<MidiMessage> OutOfRangeMessages => new(new MidiMessage[]
    {
        new MidiMessageNote(MidiChannel.Channel01, MiddleC, 128, MidiMessageType.NoteOn),
        new MidiMessageNote((MidiChannel)16, MiddleC, 1, MidiMessageType.NoteOn),
        new MidiMessageControlChange(MidiChannel.Channel01, 200, 1),
        new MidiMessageProgramChange(MidiChannel.Channel01, 0x80),
        new MidiMessageChannelAfterTouch(MidiChannel.Channel01, 0xFF),
        new MidiMessagePitchBendChange(MidiChannel.Channel01, 0, 0x80),
        new MidiMessageSongPositionPointer(0x80, 0),
        new MidiMessageSongSelect(0x80),
        new MidiMessageTimeCodeQuarterFrame(8, 0),
        new MidiMessageTimeCodeQuarterFrame(0, 16),
    });

    [Theory]
    [MemberData(nameof(OutOfRangeMessages))]
    public void ToBytes_ValueOutOfMidiRange_Throws(MidiMessage message)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiMessageConverter.ToBytes(message));
    }
}
