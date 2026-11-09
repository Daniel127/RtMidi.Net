namespace RtMidi.Net;

/// <summary>
/// A SMPTE time as carried by MIDI Time Code (MTC): hours, minutes, seconds and frames at a frame rate.
/// </summary>
/// <remarks>
/// MTC is sent either as eight Quarter Frame messages (<see cref="ToQuarterFrames"/>, while running) or as one Full
/// Message (<see cref="ToFullMessage()"/>, when locating). To read it, see
/// <see cref="MidiMessageSystemExclusive.TryGetTimeCode"/> and <see cref="MidiTimeCodeReader"/>.
/// The default value is 00:00:00:00 at 24 fps.
/// </remarks>
public readonly record struct MidiTimeCode
{
    private const int FramesPerTenMinutesDropFrame = 17982; // 10 * 60 * 30 - 9 * 2
    private const int FramesPerMinuteDropFrame = 1798;      // 60 * 30 - 2

    /// <summary>
    /// Creates a time code.
    /// </summary>
    /// <param name="hours">0 to 23.</param>
    /// <param name="minutes">0 to 59.</param>
    /// <param name="seconds">0 to 59.</param>
    /// <param name="frames">0 to the frame rate minus one (23, 24 or 29). With drop-frame, frames 0 and 1 don't exist
    /// at second 0 of a minute that isn't a multiple of 10 (e.g. 00:01:00;02 follows 00:00:59;29).</param>
    /// <param name="rate">The frame rate.</param>
    /// <exception cref="ArgumentOutOfRangeException">A value is out of range, the frame is one drop-frame skips, or
    /// <paramref name="rate"/> is not defined.</exception>
    public MidiTimeCode(int hours, int minutes, int seconds, int frames, MidiTimeCodeRate rate)
    {
        if (!Enum.IsDefined(rate)) throw new ArgumentOutOfRangeException(nameof(rate), rate, "Unknown frame rate.");
        CheckRange(hours, 23, nameof(hours));
        CheckRange(minutes, 59, nameof(minutes));
        CheckRange(seconds, 59, nameof(seconds));
        CheckRange(frames, GetFramesPerSecond(rate) - 1, nameof(frames));
        if (IsDroppedFrame(minutes, seconds, frames, rate))
        {
            throw new ArgumentOutOfRangeException(nameof(frames), frames,
                "Drop-frame skips frames 00 and 01 at the start of each minute, except every tenth minute.");
        }

        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
        Frames = frames;
        Rate = rate;
    }

    /// <summary>Hours, 0 to 23.</summary>
    public int Hours { get; }

    /// <summary>Minutes, 0 to 59.</summary>
    public int Minutes { get; }

    /// <summary>Seconds, 0 to 59.</summary>
    public int Seconds { get; }

    /// <summary>Frames, 0 to <see cref="FramesPerSecond"/> minus one.</summary>
    public int Frames { get; }

    /// <summary>The frame rate.</summary>
    public MidiTimeCodeRate Rate { get; }

    /// <summary>The number of frames in a second: 24, 25 or 30 (also 30 for drop-frame).</summary>
    public int FramesPerSecond => GetFramesPerSecond(Rate);

    /// <summary>
    /// The time code a number of frames later (or earlier, if negative), at the same rate. Drop-frame skips the frame
    /// numbers it doesn't use, and the time wraps around at 24 hours.
    /// </summary>
    /// <param name="frames">The number of frames to add.</param>
    /// <returns>The new time code.</returns>
    public MidiTimeCode AddFrames(int frames)
    {
        var fps = FramesPerSecond;
        long framesPerDay = Rate == MidiTimeCodeRate.Fps30DropFrame ? 24L * 6 * FramesPerTenMinutesDropFrame : 24L * 3600 * fps;
        var total = (ToFrameNumber() + (long)frames) % framesPerDay;
        if (total < 0) total += framesPerDay;
        return FromFrameNumber(total, Rate);
    }

    /// <summary>
    /// The eight Quarter Frame messages that carry this time, pieces 0 to 7 in the order they are sent while running
    /// forward.
    /// </summary>
    /// <remarks>
    /// Piece 0 is sent at the start of the frame this time code names, and each sequence takes two frames, so the next
    /// sequence carries the time two frames later (<see cref="AddFrames"/>).
    /// </remarks>
    /// <returns>The eight messages.</returns>
    public IReadOnlyList<MidiMessageTimeCodeQuarterFrame> ToQuarterFrames()
    {
        var hours = HoursByte();
        return
        [
            new(0, (byte)(Frames & 0x0F)),
            new(1, (byte)(Frames >> 4)),
            new(2, (byte)(Seconds & 0x0F)),
            new(3, (byte)(Seconds >> 4)),
            new(4, (byte)(Minutes & 0x0F)),
            new(5, (byte)(Minutes >> 4)),
            new(6, (byte)(hours & 0x0F)),
            new(7, (byte)(hours >> 4)),
        ];
    }

    /// <summary>
    /// The MTC Full Message with this time, addressed to every device (device ID 7FH).
    /// </summary>
    /// <returns><c>F0 7F 7F 01 01 hr mn sc fr F7</c>.</returns>
    public MidiMessageSystemExclusive ToFullMessage() => ToFullMessage(MidiMessageSystemExclusive.AllCallDeviceId);

    /// <summary>
    /// The MTC Full Message with this time.
    /// </summary>
    /// <param name="deviceId">The device ID, 0 to 127; 7FH addresses every device.</param>
    /// <returns><c>F0 7F &lt;device ID&gt; 01 01 hr mn sc fr F7</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="deviceId"/> is above 127.</exception>
    public MidiMessageSystemExclusive ToFullMessage(byte deviceId)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(deviceId, (byte)0x7F);
        return new MidiMessageSystemExclusive(
            [0xF0, 0x7F, deviceId, 0x01, 0x01, HoursByte(), (byte)Minutes, (byte)Seconds, (byte)Frames, 0xF7]);
    }

    /// <summary>
    /// The time as <c>hh:mm:ss:ff</c>, or <c>hh:mm:ss;ff</c> for drop-frame, e.g. <c>01:37:52:16</c>.
    /// </summary>
    /// <returns>The formatted time.</returns>
    public override string ToString()
    {
        var separator = Rate == MidiTimeCodeRate.Fps30DropFrame ? ';' : ':';
        return $"{Hours:D2}:{Minutes:D2}:{Seconds:D2}{separator}{Frames:D2}";
    }

    /// <summary>
    /// Decodes the hours byte (0yy zzzzz: rate and hours) and the other fields, ignoring the reserved bits.
    /// Returns false when a value is out of range.
    /// </summary>
    internal static bool TryCreate(byte hoursAndRate, byte minutes, byte seconds, byte frames, out MidiTimeCode timeCode)
    {
        var rate = (MidiTimeCodeRate)((hoursAndRate >> 5) & 0b11);
        int hours = hoursAndRate & 0x1F, minute = minutes & 0x3F, second = seconds & 0x3F, frame = frames & 0x1F;
        if (hours > 23 || minute > 59 || second > 59 || frame >= GetFramesPerSecond(rate) ||
            IsDroppedFrame(minute, second, frame, rate))
        {
            timeCode = default;
            return false;
        }

        timeCode = new MidiTimeCode(hours, minute, second, frame, rate);
        return true;
    }

    private byte HoursByte() => (byte)(((byte)Rate << 5) | Hours);

    private long ToFrameNumber()
    {
        var totalMinutes = (Hours * 60) + Minutes;
        long frameNumber = (((long)totalMinutes * 60) + Seconds) * FramesPerSecond + Frames;
        if (Rate == MidiTimeCodeRate.Fps30DropFrame) frameNumber -= 2L * (totalMinutes - (totalMinutes / 10));
        return frameNumber;
    }

    private static MidiTimeCode FromFrameNumber(long frameNumber, MidiTimeCodeRate rate)
    {
        var fps = GetFramesPerSecond(rate);
        if (rate == MidiTimeCodeRate.Fps30DropFrame)
        {
            // Add back the frame numbers drop-frame skips, then count as 30 fps.
            var tens = frameNumber / FramesPerTenMinutesDropFrame;
            var rest = frameNumber % FramesPerTenMinutesDropFrame;
            frameNumber += 18 * tens + (rest > 1 ? 2 * ((rest - 2) / FramesPerMinuteDropFrame) : 0);
        }

        var frames = (int)(frameNumber % fps);
        var totalSeconds = frameNumber / fps;
        return new MidiTimeCode((int)(totalSeconds / 3600), (int)(totalSeconds / 60 % 60), (int)(totalSeconds % 60), frames, rate);
    }

    private static bool IsDroppedFrame(int minutes, int seconds, int frames, MidiTimeCodeRate rate) =>
        rate == MidiTimeCodeRate.Fps30DropFrame && seconds == 0 && frames < 2 && minutes % 10 != 0;

    private static int GetFramesPerSecond(MidiTimeCodeRate rate) => rate switch
    {
        MidiTimeCodeRate.Fps24 => 24,
        MidiTimeCodeRate.Fps25 => 25,
        _ => 30,
    };

    private static void CheckRange(int value, int max, string name)
    {
        if (value < 0 || value > max) throw new ArgumentOutOfRangeException(name, value, $"Must be 0 to {max}.");
    }
}
