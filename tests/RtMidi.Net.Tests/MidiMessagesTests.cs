
namespace RtMidi.Net.Tests;

public class MidiMessagesTests
{
    [Theory]
    [InlineData(0x00, 0x00, 0)]
    [InlineData(0x00, 0x40, 8192)]
    [InlineData(0x7F, 0x7F, 16383)]
    [InlineData(0x01, 0x00, 1)]
    public void PitchBend_Value_Combines7BitBytes(byte lsb, byte msb, int expected)
    {
        Assert.Equal(expected, new MidiMessagePitchBendChange(MidiChannel.Channel01, lsb, msb).Value);
    }

    [Theory]
    [InlineData(0x00, 0x00, 0)]
    [InlineData(0x7F, 0x00, 127)]
    [InlineData(0x00, 0x01, 128)]
    [InlineData(0x7F, 0x7F, 16383)]
    public void SongPosition_Position_Combines7BitBytes(byte lsb, byte msb, int expected)
    {
        Assert.Equal(expected, new MidiMessageSongPositionPointer(lsb, msb).Position);
    }

    [Theory]
    [InlineData(0, 0x00, 0x00, -8192)]
    [InlineData(8192, 0x00, 0x40, 0)] // the specification's centre: LSB 00H, MSB 40H
    [InlineData(16383, 0x7F, 0x7F, 8191)]
    [InlineData(8191, 0x7F, 0x3F, -1)]
    public void PitchBend_FromValue_SplitsTheValue_AndBendIsRelativeToTheCentre(int value, byte lsb, byte msb, int bend)
    {
        var message = MidiMessagePitchBendChange.FromValue(MidiChannel.Channel03, value);

        Assert.Equal(new MidiMessagePitchBendChange(MidiChannel.Channel03, lsb, msb), message);
        Assert.Equal(value, message.Value);
        Assert.Equal(bend, message.Bend);
        Assert.Equal(message, MidiMessagePitchBendChange.FromBend(MidiChannel.Channel03, bend));
    }

    [Fact]
    public void PitchBend_RoundTripsEveryValue()
    {
        for (var value = 0; value <= MidiMessagePitchBendChange.MaxValue; value++)
        {
            var message = MidiMessagePitchBendChange.FromValue(MidiChannel.Channel01, value);
            Assert.Equal(value, message.Value);
            Assert.True(message.Lsb <= 127 && message.Msb <= 127);
            Assert.Equal(value, MidiMessagePitchBendChange.FromBend(MidiChannel.Channel01, message.Bend).Value);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16384)]
    public void PitchBend_FromValue_OutOfRange_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiMessagePitchBendChange.FromValue(MidiChannel.Channel01, value));
    }

    [Theory]
    [InlineData(-8193)]
    [InlineData(8192)]
    public void PitchBend_FromBend_OutOfRange_Throws(int bend)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiMessagePitchBendChange.FromBend(MidiChannel.Channel01, bend));
    }

    [Theory]
    [InlineData(0, 0x00, 0x00)]
    [InlineData(127, 0x7F, 0x00)]
    [InlineData(128, 0x00, 0x01)]
    [InlineData(16383, 0x7F, 0x7F)]
    public void SongPosition_FromPosition_SplitsThePosition(int position, byte lsb, byte msb)
    {
        var message = MidiMessageSongPositionPointer.FromPosition(position);

        Assert.Equal(new MidiMessageSongPositionPointer(lsb, msb), message);
        Assert.Equal(position, message.Position);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(16384)]
    public void SongPosition_FromPosition_OutOfRange_Throws(int position)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiMessageSongPositionPointer.FromPosition(position));
    }

    [Fact]
    public void Records_HaveDefaultTypes()
    {
        var note = new MidiNote(60);
        Assert.Equal(MidiMessageType.PolyphonicKeyPressure, new MidiMessageNoteAfterTouch(MidiChannel.Channel01, note, 1).Type);
        Assert.Equal(MidiMessageType.ControlChange, new MidiMessageControlChange(MidiChannel.Channel01, MidiControlFunction.ModulationWheel, 1).Type);
        Assert.Equal(MidiMessageType.ProgramChange, new MidiMessageProgramChange(MidiChannel.Channel01, 1).Type);
        Assert.Equal(MidiMessageType.ChannelPressure, new MidiMessageChannelAfterTouch(MidiChannel.Channel01, 1).Type);
        Assert.Equal(MidiMessageType.PitchBendChange, new MidiMessagePitchBendChange(MidiChannel.Channel01, 0, 0).Type);
        Assert.Equal(MidiMessageType.SystemExclusive, new MidiMessageSystemExclusive([]).Type);
        Assert.Equal(MidiMessageType.TimeCodeQuarterFrame, new MidiMessageTimeCodeQuarterFrame(0, 0).Type);
        Assert.Equal(MidiMessageType.SongPositionPointer, new MidiMessageSongPositionPointer(0, 0).Type);
        Assert.Equal(MidiMessageType.SongSelect, new MidiMessageSongSelect(0).Type);
        Assert.Equal(MidiMessageType.Unknown, new MidiMessageUnknown([]).Type);
    }

    [Fact]
    public void ReceivedEventArgs_KeepsMessageAndTimestamp()
    {
        var message = new MidiMessage(MidiMessageType.Start);
        var args = new MidiMessageReceivedEventArgs(message, TimeSpan.FromMilliseconds(5));

        Assert.Same(message, args.Message);
        Assert.Equal(TimeSpan.FromMilliseconds(5), args.Timestamp);
    }

    [Fact]
    public void RtMidiException_KeepsMessage()
    {
        Assert.Equal("boom", new RtMidiException("boom").Message);
    }

    [Fact]
    public void DeviceInfo_Deconstructs()
    {
        var (name, port, api, type) = new MidiDeviceInfo("Port", 3, MidiApi.LinuxAlsa, MidiDeviceType.Output);

        Assert.Equal(("Port", 3u, MidiApi.LinuxAlsa, MidiDeviceType.Output), (name, port, api, type));
    }
}
