namespace RtMidi.Net.Tests;

public class MidiTimeCodeReaderTests
{
    private readonly MidiTimeCodeReader _reader = new();

    [Fact]
    public void Forward_CompletesOnPiece7_WithTwoFrameOffset()
    {
        var timeCode = new MidiTimeCode(1, 37, 52, 16, MidiTimeCodeRate.Fps30);
        var pieces = timeCode.ToQuarterFrames();

        for (var i = 0; i < 7; i++) Assert.False(_reader.TryDecode(pieces[i], out _));
        Assert.True(_reader.TryDecode(pieces[7], out var update));

        Assert.Equal(new MidiTimeCodeUpdate(timeCode.AddFrames(2), MidiTimeCodeSource.QuarterFramesForward), update);
    }

    [Fact]
    public void Forward_ConsecutiveSequences_EachGiveTheTime()
    {
        var timeCode = new MidiTimeCode(0, 0, 59, 28, MidiTimeCodeRate.Fps30DropFrame);
        for (var sequence = 0; sequence < 3; sequence++)
        {
            var update = Feed(timeCode.ToQuarterFrames());
            Assert.Equal(timeCode.AddFrames(2), update?.TimeCode);
            timeCode = timeCode.AddFrames(2);
        }

        Assert.Equal("00:01:00;06", timeCode.ToString()); // 59;28 + 6 frames, skipping 00 and 01
    }

    [Fact]
    public void Reverse_CompletesOnPiece0_WithoutOffset()
    {
        var timeCode = new MidiTimeCode(10, 0, 0, 3, MidiTimeCodeRate.Fps25);
        var pieces = timeCode.ToQuarterFrames().Reverse().ToList();

        var update = Feed(pieces);

        Assert.Equal(new MidiTimeCodeUpdate(timeCode, MidiTimeCodeSource.QuarterFramesReverse), update);
    }

    [Fact]
    public void ComingOnLineMidSequence_WaitsForTheNextOne()
    {
        var first = new MidiTimeCode(0, 0, 10, 0, MidiTimeCodeRate.Fps24);
        Assert.Null(Feed(first.ToQuarterFrames().Skip(3)));

        var second = first.AddFrames(2);
        Assert.Equal(second.AddFrames(2), Feed(second.ToQuarterFrames())?.TimeCode);
    }

    [Fact]
    public void MissingPiece_DropsTheSequence()
    {
        var pieces = new MidiTimeCode(0, 0, 10, 0, MidiTimeCodeRate.Fps24).ToQuarterFrames().ToList();
        pieces.RemoveAt(4);

        Assert.Null(Feed(pieces));
    }

    [Fact]
    public void DirectionChange_StartsANewSequence()
    {
        var timeCode = new MidiTimeCode(0, 0, 10, 0, MidiTimeCodeRate.Fps24);
        var forward = timeCode.ToQuarterFrames();
        Assert.Null(Feed(forward.Take(4)));

        var update = Feed(forward.Reverse());

        Assert.Equal(MidiTimeCodeSource.QuarterFramesReverse, update?.Source);
        Assert.Equal(timeCode, update?.TimeCode);
    }

    [Fact]
    public void Piece0_RestartsAForwardSequence()
    {
        var timeCode = new MidiTimeCode(0, 0, 10, 0, MidiTimeCodeRate.Fps24);
        var pieces = timeCode.ToQuarterFrames();
        Assert.Null(Feed(pieces.Take(5)));

        Assert.Equal(timeCode.AddFrames(2), Feed(pieces)?.TimeCode);
    }

    [Fact]
    public void FullMessage_GivesTheTimeAndDropsTheSequence()
    {
        var running = new MidiTimeCode(0, 0, 10, 0, MidiTimeCodeRate.Fps24);
        Assert.Null(Feed(running.ToQuarterFrames().Take(4)));

        var located = new MidiTimeCode(2, 0, 0, 0, MidiTimeCodeRate.Fps24);
        Assert.True(_reader.TryDecode(located.ToFullMessage(), out var update));
        Assert.Equal(new MidiTimeCodeUpdate(located, MidiTimeCodeSource.FullMessage), update);

        // The rest of the interrupted sequence gives nothing.
        Assert.Null(Feed(running.ToQuarterFrames().Skip(4)));
    }

    [Fact]
    public void Reset_DropsTheSequence()
    {
        var pieces = new MidiTimeCode(0, 0, 10, 0, MidiTimeCodeRate.Fps24).ToQuarterFrames();
        Assert.Null(Feed(pieces.Take(4)));
        _reader.Reset();

        Assert.Null(Feed(pieces.Skip(4)));
    }

    [Fact]
    public void InvalidTime_GivesNothing()
    {
        // 30 frames at 24 fps.
        var pieces = new MidiTimeCode(0, 0, 0, 29, MidiTimeCodeRate.Fps30).ToQuarterFrames().ToArray();
        pieces[7] = new MidiMessageTimeCodeQuarterFrame(7, 0);

        Assert.Null(Feed(pieces));
    }

    [Fact]
    public void OtherMessages_AreIgnoredAndDoNotBreakTheSequence()
    {
        var timeCode = new MidiTimeCode(0, 0, 10, 0, MidiTimeCodeRate.Fps24);
        var pieces = timeCode.ToQuarterFrames();
        Assert.Null(Feed(pieces.Take(4)));

        Assert.False(_reader.TryDecode(new MidiMessage(MidiMessageType.TimingClock), out _));
        Assert.False(_reader.TryDecode(MidiMessageSystemExclusive.IdentityRequest(), out _));
        Assert.False(_reader.TryDecode(new MidiMessageNote(MidiChannel.Channel01, MidiNote.Parse("C4"), 100, MidiMessageType.NoteOn), out _));

        Assert.Equal(timeCode.AddFrames(2), Feed(pieces.Skip(4))?.TimeCode);
    }

    [Fact]
    public void ParsedBytes_AreRead()
    {
        byte[][] bytes =
        [
            [0xF1, 0x00], [0xF1, 0x11], [0xF1, 0x24], [0xF1, 0x33],
            [0xF1, 0x45], [0xF1, 0x52], [0xF1, 0x61], [0xF1, 0x76],
        ];

        var update = Feed(bytes.Select(b => MidiMessageConverter.Parse(b)));

        Assert.Equal("01:37:52:18", update?.TimeCode.ToString());
        Assert.Equal(MidiTimeCodeRate.Fps30, update?.TimeCode.Rate);
    }

    [Fact]
    public void TryDecode_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _reader.TryDecode(null!, out _));
    }

    private MidiTimeCodeUpdate? Feed(IEnumerable<MidiMessage> messages)
    {
        MidiTimeCodeUpdate? last = null;
        foreach (var message in messages)
        {
            if (_reader.TryDecode(message, out var update)) last = update;
        }

        return last;
    }
}
