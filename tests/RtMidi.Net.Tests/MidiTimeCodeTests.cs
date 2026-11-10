namespace RtMidi.Net.Tests;

public class MidiTimeCodeTests
{
    [Theory]
    [InlineData(-1, 0, 0, 0, MidiTimeCodeRate.Fps24)]
    [InlineData(24, 0, 0, 0, MidiTimeCodeRate.Fps24)]
    [InlineData(0, 60, 0, 0, MidiTimeCodeRate.Fps24)]
    [InlineData(0, 0, 60, 0, MidiTimeCodeRate.Fps24)]
    [InlineData(0, 0, 0, 24, MidiTimeCodeRate.Fps24)]
    [InlineData(0, 0, 0, 25, MidiTimeCodeRate.Fps25)]
    [InlineData(0, 0, 0, 30, MidiTimeCodeRate.Fps30)]
    [InlineData(0, 0, 0, -1, MidiTimeCodeRate.Fps30)]
    [InlineData(0, 0, 0, 0, (MidiTimeCodeRate)4)]
    [InlineData(0, 1, 0, 0, MidiTimeCodeRate.Fps30DropFrame)]
    [InlineData(0, 1, 0, 1, MidiTimeCodeRate.Fps30DropFrame)]
    [InlineData(5, 59, 0, 1, MidiTimeCodeRate.Fps30DropFrame)]
    public void Constructor_OutOfRange_Throws(int hours, int minutes, int seconds, int frames, MidiTimeCodeRate rate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MidiTimeCode(hours, minutes, seconds, frames, rate));
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(0, 10, 0, 0)]
    [InlineData(0, 1, 0, 2)]
    [InlineData(0, 1, 1, 0)]
    public void Constructor_DropFrame_AcceptsFramesThatExist(int hours, int minutes, int seconds, int frames)
    {
        var timeCode = new MidiTimeCode(hours, minutes, seconds, frames, MidiTimeCodeRate.Fps30DropFrame);
        Assert.Equal(timeCode, timeCode.AddFrames(0));
    }

    [Theory]
    [InlineData(MidiTimeCodeRate.Fps24, 24)]
    [InlineData(MidiTimeCodeRate.Fps25, 25)]
    [InlineData(MidiTimeCodeRate.Fps30DropFrame, 30)]
    [InlineData(MidiTimeCodeRate.Fps30, 30)]
    public void FramesPerSecond_MatchesRate(MidiTimeCodeRate rate, int expected)
    {
        var timeCode = new MidiTimeCode(23, 59, 59, expected - 1, rate);
        Assert.Equal(expected, timeCode.FramesPerSecond);
        Assert.Equal(expected - 1, timeCode.Frames);
    }

    [Fact]
    public void Default_IsMidnightAt24Fps()
    {
        Assert.Equal(new MidiTimeCode(0, 0, 0, 0, MidiTimeCodeRate.Fps24), default);
    }

    [Theory]
    [InlineData(1, 37, 52, 16, MidiTimeCodeRate.Fps30, "01:37:52:16")]
    [InlineData(0, 1, 0, 2, MidiTimeCodeRate.Fps30DropFrame, "00:01:00;02")]
    [InlineData(23, 0, 9, 0, MidiTimeCodeRate.Fps25, "23:00:09:00")]
    public void ToString_FormatsTime(int h, int m, int s, int f, MidiTimeCodeRate rate, string expected)
    {
        Assert.Equal(expected, new MidiTimeCode(h, m, s, f, rate).ToString());
    }

    [Theory]
    [InlineData("00:00:00:23", MidiTimeCodeRate.Fps24, 1, "00:00:01:00")]
    [InlineData("00:00:00:24", MidiTimeCodeRate.Fps25, 1, "00:00:01:00")]
    [InlineData("00:59:59:29", MidiTimeCodeRate.Fps30, 1, "01:00:00:00")]
    [InlineData("23:59:59:29", MidiTimeCodeRate.Fps30, 1, "00:00:00:00")]
    [InlineData("00:00:00:00", MidiTimeCodeRate.Fps24, -1, "23:59:59:23")]
    [InlineData("01:37:52:16", MidiTimeCodeRate.Fps30, 2, "01:37:52:18")]
    [InlineData("00:00:01:00", MidiTimeCodeRate.Fps30, -2, "00:00:00:28")]
    [InlineData("00:00:00:00", MidiTimeCodeRate.Fps25, 90_000, "01:00:00:00")]
    // Drop-frame skips frames 00 and 01 at the start of each minute, except every tenth minute.
    [InlineData("00:00:59;29", MidiTimeCodeRate.Fps30DropFrame, 1, "00:01:00;02")]
    [InlineData("00:01:00;02", MidiTimeCodeRate.Fps30DropFrame, -1, "00:00:59;29")]
    [InlineData("00:09:59;29", MidiTimeCodeRate.Fps30DropFrame, 1, "00:10:00;00")]
    [InlineData("00:00:00;00", MidiTimeCodeRate.Fps30DropFrame, 1800, "00:01:00;02")]
    [InlineData("00:00:00;00", MidiTimeCodeRate.Fps30DropFrame, 17982, "00:10:00;00")]
    [InlineData("00:00:00;00", MidiTimeCodeRate.Fps30DropFrame, 107892, "01:00:00;00")]
    [InlineData("23:59:59;29", MidiTimeCodeRate.Fps30DropFrame, 1, "00:00:00;00")]
    [InlineData("00:00:00;00", MidiTimeCodeRate.Fps30DropFrame, -1, "23:59:59;29")]
    public void AddFrames_CountsFramesAtTheRate(string start, MidiTimeCodeRate rate, int frames, string expected)
    {
        Assert.Equal(expected, Parse(start, rate).AddFrames(frames).ToString());
    }

    [Fact]
    public void AddFrames_DropFrame_RoundTripsEveryFrameOfAnHour()
    {
        var start = new MidiTimeCode(0, 0, 0, 0, MidiTimeCodeRate.Fps30DropFrame);
        var current = start;
        for (var i = 1; i <= 107892; i++)
        {
            var next = current.AddFrames(1);
            Assert.Equal(current, next.AddFrames(-1));
            Assert.False(next.Seconds == 0 && next.Frames < 2 && next.Minutes % 10 != 0, $"{next} is a dropped frame");
            Assert.Equal(next, start.AddFrames(i));
            current = next;
        }

        Assert.Equal("01:00:00;00", current.ToString());
    }

    [Fact]
    public void ToQuarterFrames_MatchesSpecificationExample()
    {
        // MTC specification 4.2.1, page 3: 01:37:52:16 at 30 fps non-drop.
        var bytes = new MidiTimeCode(1, 37, 52, 16, MidiTimeCodeRate.Fps30).ToQuarterFrames()
            .Select(MidiMessageConverter.ToBytes).ToArray();

        byte[][] expected =
        [
            [0xF1, 0x00], [0xF1, 0x11], [0xF1, 0x24], [0xF1, 0x33],
            [0xF1, 0x45], [0xF1, 0x52], [0xF1, 0x61], [0xF1, 0x76],
        ];
        Assert.Equal(expected, bytes);
    }

    [Theory]
    [InlineData(MidiTimeCodeRate.Fps24, 0x00)]
    [InlineData(MidiTimeCodeRate.Fps25, 0x02)]
    [InlineData(MidiTimeCodeRate.Fps30DropFrame, 0x04)]
    [InlineData(MidiTimeCodeRate.Fps30, 0x06)]
    public void ToQuarterFrames_Piece7_CarriesRate(MidiTimeCodeRate rate, byte expected)
    {
        var piece7 = new MidiTimeCode(17, 0, 0, 0, rate).ToQuarterFrames()[7];
        Assert.Equal(7, piece7.Piece);
        Assert.Equal(expected | 0x01, piece7.Value); // 17 = 10001b: the 5th hour bit goes to piece 7
    }

    [Fact]
    public void ToFullMessage_EncodesTime()
    {
        var timeCode = new MidiTimeCode(1, 37, 52, 16, MidiTimeCodeRate.Fps30);
        Assert.Equal(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x61, 37, 52, 16, 0xF7 }, timeCode.ToFullMessage().Data);
        Assert.Equal(new byte[] { 0xF0, 0x7F, 0x05, 0x01, 0x01, 0x61, 37, 52, 16, 0xF7 }, timeCode.ToFullMessage(5).Data);
        Assert.Throws<ArgumentOutOfRangeException>(() => timeCode.ToFullMessage(0x80));
    }

    [Fact]
    public void TryGetTimeCode_ReadsFullMessage()
    {
        var timeCode = new MidiTimeCode(12, 34, 56, 21, MidiTimeCodeRate.Fps30DropFrame);
        Assert.True(timeCode.ToFullMessage(3).TryGetTimeCode(out var read));
        Assert.Equal(timeCode, read);
    }

    [Theory]
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x61, 37, 52, 16 })]                  // no F7
    [InlineData(new byte[] { 0xF0, 0x7E, 0x7F, 0x01, 0x01, 0x61, 37, 52, 16, 0xF7 })]            // non-real time
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x02, 0x61, 37, 52, 16, 0xF7 })]            // user bits
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x78, 37, 52, 16, 0xF7 })]            // hour 24
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x01, 60, 52, 16, 0xF7 })]            // minute 60
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x01, 37, 60, 16, 0xF7 })]            // second 60
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x01, 37, 52, 24, 0xF7 })]            // frame 24 at 24 fps
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x61, 37, 52, 16, 0x00, 0xF7 })]      // too long
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x00, 0x80, 0x00, 0x00, 0xF7 })]      // status byte as minutes
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x80, 0x00, 0x00, 0x00, 0xF7 })]      // status byte as hours
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x00, 0x00, 0x80, 0x00, 0xF7 })]      // status byte as seconds
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x00, 0x00, 0x00, 0x80, 0xF7 })]      // status byte as frames
    [InlineData(new byte[] { 0xF0, 0x7F, 0x80, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0xF7 })]      // status byte as device
    [InlineData(new byte[] { 0xF0, 0x7F, 0x7F, 0x01, 0x01, 0x41, 0x01, 0x00, 0x00, 0xF7 })]      // dropped frame 00:01:00;00
    public void TryGetTimeCode_RejectsOtherMessages(byte[] data)
    {
        Assert.False(new MidiMessageSystemExclusive(data).TryGetTimeCode(out var timeCode));
        Assert.Equal(default, timeCode);
    }

    internal static MidiTimeCode Parse(string text, MidiTimeCodeRate rate)
    {
        var parts = text.Split(':', ';').Select(int.Parse).ToArray();
        return new MidiTimeCode(parts[0], parts[1], parts[2], parts[3], rate);
    }
}
