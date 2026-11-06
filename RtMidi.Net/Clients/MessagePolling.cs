namespace RtMidi.Net.Clients;

/// <summary>
/// Waits for a message from RtMidi's input queue, which can only be polled.
/// </summary>
internal static class MessagePolling
{
    /// <summary>
    /// Pause between two polls of an empty queue, so waiting does not keep a CPU core busy.
    /// </summary>
    internal static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(1);

    /// <summary>
    /// Reads one message from the queue: the bytes (empty when the queue is empty) and the delta-time in seconds.
    /// </summary>
    internal delegate (byte[] message, double timestamp) QueueReader();

    public static bool TryRead(QueueReader read, out MidiMessage? message, out TimeSpan timestamp)
    {
        var (data, seconds) = read();
        if (data.Length == 0)
        {
            message = null;
            timestamp = TimeSpan.Zero;
            return false;
        }

        message = MidiMessageConverter.Parse(data);
        timestamp = TimeSpan.FromSeconds(seconds);
        return true;
    }

    public static (MidiMessage message, TimeSpan timestamp) Wait(QueueReader read, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TryRead(read, out var message, out var timestamp)) return (message!, timestamp);
            cancellationToken.WaitHandle.WaitOne(Interval);
        }
    }

    public static async Task<(MidiMessage message, TimeSpan timestamp)> WaitAsync(QueueReader read, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TryRead(read, out var message, out var timestamp)) return (message!, timestamp);
            await Task.Delay(Interval, cancellationToken).ConfigureAwait(false);
        }
    }
}
