using System.Diagnostics;

namespace RtMidi.Net.Tests;

public class MessagePollingTests
{
    /// <summary>
    /// Fake RtMidi queue: returns empty reads until the given messages are due.
    /// </summary>
    private sealed class FakeQueue(int emptyReadsBeforeEachMessage, params (byte[] message, double timestamp)[] messages)
    {
        private readonly Queue<(byte[], double)> _messages = new(messages);
        private int _emptyReads;

        public int Reads { get; private set; }

        public (byte[] message, double timestamp) Read()
        {
            Reads++;
            if (_messages.Count == 0 || _emptyReads < emptyReadsBeforeEachMessage)
            {
                _emptyReads++;
                return ([], 0);
            }

            _emptyReads = 0;
            return _messages.Dequeue();
        }
    }

    [Fact]
    public void TryRead_EmptyQueue_ReturnsFalse()
    {
        var queue = new FakeQueue(0);

        Assert.False(MessagePolling.TryRead(queue.Read, out var message, out var timestamp));
        Assert.Null(message);
        Assert.Equal(TimeSpan.Zero, timestamp);
    }

    [Fact]
    public void TryRead_Message_ParsesItAndUsesRtMidiTimestamp()
    {
        var queue = new FakeQueue(0, ([0x90, 60, 100], 0.25));

        Assert.True(MessagePolling.TryRead(queue.Read, out var message, out var timestamp));
        Assert.Equal(new MidiMessageNote(MidiChannel.Channel01, new MidiNote(60), 100, MidiMessageType.NoteOn), message);
        Assert.Equal(TimeSpan.FromSeconds(0.25), timestamp);
    }

    [Fact]
    public void Wait_ReturnsFirstMessageAfterEmptyReads()
    {
        var queue = new FakeQueue(3, ([0xF8], 0.5));

        var (message, timestamp) = MessagePolling.Wait(queue.Read, CancellationToken.None);

        Assert.Equal(MidiMessageType.TimingClock, message.Type);
        Assert.Equal(TimeSpan.FromSeconds(0.5), timestamp);
        Assert.Equal(4, queue.Reads);
    }

    [Fact]
    public void Wait_EmptyQueue_DoesNotSpin()
    {
        var queue = new FakeQueue(0);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        Assert.ThrowsAny<OperationCanceledException>(() => MessagePolling.Wait(queue.Read, cts.Token));

        // A busy-wait would poll millions of times in 200 ms; with the pause it is at most a few hundred.
        Assert.InRange(queue.Reads, 1, 1000);
    }

    [Fact]
    public void Wait_AlreadyCancelled_ThrowsWithoutReading()
    {
        var queue = new FakeQueue(0, ([0xF8], 0));

        Assert.ThrowsAny<OperationCanceledException>(() => MessagePolling.Wait(queue.Read, new CancellationToken(true)));
        Assert.Equal(0, queue.Reads);
    }

    [Fact]
    public async Task WaitAsync_ReturnsMessagesInOrder()
    {
        var queue = new FakeQueue(2, ([0xFA], 0.0), ([0xFC], 1.5));

        var first = await MessagePolling.WaitAsync(queue.Read, TestContext.Current.CancellationToken);
        var second = await MessagePolling.WaitAsync(queue.Read, TestContext.Current.CancellationToken);

        Assert.Equal(MidiMessageType.Start, first.message.Type);
        Assert.Equal(MidiMessageType.Stop, second.message.Type);
        Assert.Equal(TimeSpan.FromSeconds(1.5), second.timestamp);
    }

    [Fact]
    public async Task WaitAsync_EmptyQueue_HonoursCancellationAndDoesNotSpin()
    {
        var queue = new FakeQueue(0);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => MessagePolling.WaitAsync(queue.Read, cts.Token));

        Assert.InRange(stopwatch.Elapsed, TimeSpan.FromMilliseconds(150), TimeSpan.FromSeconds(10));
        Assert.InRange(queue.Reads, 1, 1000);
    }
}
