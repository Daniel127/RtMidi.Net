
namespace RtMidi.Net.Tests;

public class MidiMessageConverterParseTests
{
    [Theory]
    [InlineData(0x90, MidiMessageType.NoteOn, MidiChannel.Channel01)]
    [InlineData(0x9F, MidiMessageType.NoteOn, MidiChannel.Channel16)]
    [InlineData(0x80, MidiMessageType.NoteOff, MidiChannel.Channel01)]
    [InlineData(0x8A, MidiMessageType.NoteOff, MidiChannel.Channel11)]
    public void Parse_NoteMessage_ReturnsNoteWithChannelNoteAndVelocity(byte status, MidiMessageType type, MidiChannel channel)
    {
        var message = MidiMessageConverter.Parse([status, 60, 100]);

        var note = Assert.IsType<MidiMessageNote>(message);
        Assert.Equal(type, note.Type);
        Assert.Equal(channel, note.Channel);
        Assert.Equal(MusicNote.C, note.Note.Note);
        Assert.Equal(MusicOctave.Octave4, note.Note.Octave);
        Assert.Equal(100, note.Velocity);
    }

    [Fact]
    public void Parse_NoteOnWithZeroVelocity_StaysNoteOn()
    {
        var note = Assert.IsType<MidiMessageNote>(MidiMessageConverter.Parse([0x90, 64, 0]));

        Assert.Equal(MidiMessageType.NoteOn, note.Type);
        Assert.Equal(0, note.Velocity);
    }

    [Fact]
    public void Parse_PolyphonicKeyPressure()
    {
        var message = Assert.IsType<MidiMessageNoteAfterTouch>(MidiMessageConverter.Parse([0xA3, 127, 55]));

        Assert.Equal(MidiMessageType.PolyphonicKeyPressure, message.Type);
        Assert.Equal(MidiChannel.Channel04, message.Channel);
        Assert.Equal(127, message.Note.Number);
        Assert.Equal(55, message.Pressure);
    }

    [Fact]
    public void Parse_ControlChange()
    {
        var message = MidiMessageConverter.Parse([0xB0, 7, 99]);

        Assert.Equal(new MidiMessageControlChange(MidiChannel.Channel01, 7, 99), message);
    }

    [Fact]
    public void Parse_ProgramChange()
    {
        var message = MidiMessageConverter.Parse([0xC5, 12]);

        Assert.Equal(new MidiMessageProgramChange(MidiChannel.Channel06, 12), message);
    }

    [Fact]
    public void Parse_ChannelPressure()
    {
        var message = MidiMessageConverter.Parse([0xDF, 33]);

        Assert.Equal(new MidiMessageChannelAfterTouch(MidiChannel.Channel16, 33), message);
    }

    [Fact]
    public void Parse_PitchBend()
    {
        var message = Assert.IsType<MidiMessagePitchBendChange>(MidiMessageConverter.Parse([0xE2, 0x00, 0x40]));

        Assert.Equal(MidiChannel.Channel03, message.Channel);
        Assert.Equal(0x00, message.Lsb);
        Assert.Equal(0x40, message.Msb);
        Assert.Equal(8192, message.Value);
    }

    [Fact]
    public void Parse_SystemExclusive_KeepsWholeMessage()
    {
        byte[] data = [0xF0, 0x7E, 0x7F, 0x06, 0x01, 0xF7];

        var message = Assert.IsType<MidiMessageSystemExclusive>(MidiMessageConverter.Parse(data));

        Assert.Equal(data, message.Data);
        Assert.Equal(MidiMessageType.SystemExclusive, message.Type);
    }

    [Theory]
    [InlineData(0x00, 0, 0)]
    [InlineData(0x1F, 1, 15)]
    [InlineData(0x35, 3, 5)]
    [InlineData(0x76, 7, 6)]
    public void Parse_TimeCodeQuarterFrame_SplitsPieceAndValue(byte data, byte piece, byte value)
    {
        var message = MidiMessageConverter.Parse([0xF1, data]);

        Assert.Equal(new MidiMessageTimeCodeQuarterFrame(piece, value), message);
    }

    [Fact]
    public void Parse_SongPositionPointer()
    {
        var message = Assert.IsType<MidiMessageSongPositionPointer>(MidiMessageConverter.Parse([0xF2, 0x01, 0x02]));

        Assert.Equal(0x01, message.Lsb);
        Assert.Equal(0x02, message.Msb);
        Assert.Equal(257, message.Position);
    }

    [Fact]
    public void Parse_SongSelect()
    {
        Assert.Equal(new MidiMessageSongSelect(9), MidiMessageConverter.Parse([0xF3, 9]));
    }

    [Theory]
    [InlineData(MidiMessageType.UndefinedReserved1)]
    [InlineData(MidiMessageType.UndefinedReserved2)]
    [InlineData(MidiMessageType.TuneRequest)]
    [InlineData(MidiMessageType.EndOfSysEx)]
    [InlineData(MidiMessageType.TimingClock)]
    [InlineData(MidiMessageType.UndefinedReserved3)]
    [InlineData(MidiMessageType.Start)]
    [InlineData(MidiMessageType.Continue)]
    [InlineData(MidiMessageType.Stop)]
    [InlineData(MidiMessageType.UndefinedReserved4)]
    [InlineData(MidiMessageType.ActiveSensing)]
    [InlineData(MidiMessageType.SystemReset)]
    public void Parse_SingleByteSystemMessage_ReturnsBaseMessage(MidiMessageType type)
    {
        var message = MidiMessageConverter.Parse([(byte)type]);

        Assert.Equal(typeof(MidiMessage), message.GetType());
        Assert.Equal(type, message.Type);
    }

    public static TheoryData<byte[]> MalformedMessages => new()
    {
        Array.Empty<byte>(),
        new byte[] { 0x40, 0x10 },        // starts with a data byte
        new byte[] { 0x90 },              // Note On without data
        new byte[] { 0x90, 60 },          // Note On without velocity
        new byte[] { 0xC0 },              // Program Change without program
        new byte[] { 0xE0, 0x00 },        // Pitch Bend without MSB
        new byte[] { 0x90, 0x80, 0x10 },  // data byte with the high bit set
        new byte[] { 0xB0, 0x07, 0xFF },
        new byte[] { 0xF1 },              // Quarter Frame without data
        new byte[] { 0xF1, 0x80 },
        new byte[] { 0xF2, 0x00 },        // Song Position without MSB
        new byte[] { 0xF3 },              // Song Select without song
    };

    [Theory]
    [MemberData(nameof(MalformedMessages))]
    public void Parse_MalformedMessage_ReturnsUnknownWithRawBytes(byte[] data)
    {
        var message = Assert.IsType<MidiMessageUnknown>(MidiMessageConverter.Parse(data));

        Assert.Equal(MidiMessageType.Unknown, message.Type);
        Assert.Equal(data, message.MessageData);
    }

    [Fact]
    public void Parse_ExtraBytes_AreIgnored()
    {
        var message = MidiMessageConverter.Parse([0x90, 60, 100, 0x12]);

        Assert.IsType<MidiMessageNote>(message);
    }

    [Theory]
    [InlineData(MidiMessageType.NoteOff, 2)]
    [InlineData(MidiMessageType.NoteOn, 2)]
    [InlineData(MidiMessageType.PolyphonicKeyPressure, 2)]
    [InlineData(MidiMessageType.ControlChange, 2)]
    [InlineData(MidiMessageType.ProgramChange, 1)]
    [InlineData(MidiMessageType.ChannelPressure, 1)]
    [InlineData(MidiMessageType.PitchBendChange, 2)]
    [InlineData(MidiMessageType.TimeCodeQuarterFrame, 1)]
    [InlineData(MidiMessageType.SongPositionPointer, 2)]
    [InlineData(MidiMessageType.SongSelect, 1)]
    [InlineData(MidiMessageType.TuneRequest, 0)]
    [InlineData(MidiMessageType.TimingClock, 0)]
    public void DataLength_MatchesMidiSpecification(MidiMessageType type, int expected)
    {
        Assert.Equal(expected, MidiMessageConverter.DataLength(type));
    }
}
