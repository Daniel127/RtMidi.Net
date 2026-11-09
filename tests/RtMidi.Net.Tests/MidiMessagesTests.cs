
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
