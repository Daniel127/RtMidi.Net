using System.Diagnostics.CodeAnalysis;

namespace RtMidi.Net;

/// <summary>
/// Assembles the time from received MIDI Time Code: Quarter Frame sequences and Full Messages.
/// </summary>
/// <remarks>
/// <para>
/// Pass every received message to <see cref="TryDecode"/>, in order. It returns <see langword="true"/> with a
/// <see cref="MidiTimeCodeUpdate"/> when a message completes a time, and <see langword="false"/> for the rest.
/// It follows the MIDI Time Code specification (MTC 4.2.1, pages 2 and 3):
/// </para>
/// <list type="bullet">
/// <item>A time needs the eight Quarter Frame pieces in a row: 0 to 7 while running forward, 7 to 0 in reverse. A
/// piece out of order drops the sequence; piece 0 or 7 starts a new one.</item>
/// <item>Forward, the time is complete on piece 7, when it is two frames old, so the update carries it two frames
/// later. In reverse, piece 0 falls on the boundary of the frame it carries, so the time is used as is.</item>
/// <item>A Full Message gives the time at once and drops the current sequence.</item>
/// </list>
/// <para>
/// The way a reader handles sequences that break or change direction is not fully defined by the specification,
/// which is why this class is experimental: its rules or shape may change in a minor version. Not thread-safe: use
/// one instance per input, from one thread (e.g. the <see cref="MidiInputClient.MessageReceived"/> handler).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var reader = new MidiTimeCodeReader();
/// input.MessageReceived += (_, e) =&gt;
/// {
///     if (reader.TryDecode(e.Message, out var update))
///         Console.WriteLine(update.TimeCode);
/// };
/// </code>
/// </example>
[Experimental(DiagnosticId)]
public sealed class MidiTimeCodeReader
{
    /// <summary>
    /// The diagnostic id of the experimental MIDI Time Code reader API. Suppress it (e.g.
    /// <c>&lt;NoWarn&gt;RTMIDI002&lt;/NoWarn&gt;</c>) to use the API.
    /// </summary>
    public const string DiagnosticId = "RTMIDI002";

    private const int PieceCount = 8;
    private const int ForwardOffsetFrames = 2;

    private readonly byte[] _pieces = new byte[PieceCount];
    private int _direction; // 1 forward, -1 reverse, 0 waiting for piece 0 or 7
    private int _expectedPiece;

    /// <summary>
    /// Reads one message.
    /// </summary>
    /// <param name="message">The received message; messages that aren't MIDI Time Code are ignored.</param>
    /// <param name="update">The time, when this message completes one.</param>
    /// <returns><see langword="true"/> if <paramref name="update"/> has a new time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
    public bool TryDecode(MidiMessage message, [NotNullWhen(true)] out MidiTimeCodeUpdate? update)
    {
        ArgumentNullException.ThrowIfNull(message);
        update = null;
        switch (message)
        {
            case MidiMessageTimeCodeQuarterFrame quarterFrame:
                return TryAddPiece(quarterFrame.Piece & 0x07, (byte)(quarterFrame.Value & 0x0F), out update);
            case MidiMessageSystemExclusive sysEx when sysEx.TryGetTimeCode(out var timeCode):
                Reset();
                update = new MidiTimeCodeUpdate(timeCode, MidiTimeCodeSource.FullMessage);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Drops the Quarter Frame sequence in progress.
    /// </summary>
    public void Reset()
    {
        _direction = 0;
        Array.Clear(_pieces);
    }

    private bool TryAddPiece(int piece, byte value, out MidiTimeCodeUpdate? update)
    {
        update = null;
        if (_direction == 0 || piece != _expectedPiece)
        {
            // Out of order: start again if this piece can open a sequence.
            Reset();
            if (piece == 0) _direction = 1;
            else if (piece == PieceCount - 1) _direction = -1;
            else return false;
        }

        _pieces[piece] = value;
        var isLast = _direction > 0 ? piece == PieceCount - 1 : piece == 0;
        if (!isLast)
        {
            _expectedPiece = piece + _direction;
            return false;
        }

        var forward = _direction > 0;
        _direction = 0;
        if (!MidiTimeCode.TryCreate(
                (byte)((_pieces[7] << 4) | _pieces[6]),
                (byte)((_pieces[5] << 4) | _pieces[4]),
                (byte)((_pieces[3] << 4) | _pieces[2]),
                (byte)((_pieces[1] << 4) | _pieces[0]),
                out var timeCode))
        {
            return false;
        }

        update = forward
            ? new MidiTimeCodeUpdate(timeCode.AddFrames(ForwardOffsetFrames), MidiTimeCodeSource.QuarterFramesForward)
            : new MidiTimeCodeUpdate(timeCode, MidiTimeCodeSource.QuarterFramesReverse);
        return true;
    }
}

/// <summary>
/// A time assembled by <see cref="MidiTimeCodeReader"/>.
/// </summary>
/// <param name="TimeCode">The current time.</param>
/// <param name="Source">Where it came from.</param>
[Experimental(MidiTimeCodeReader.DiagnosticId)]
public sealed record MidiTimeCodeUpdate(MidiTimeCode TimeCode, MidiTimeCodeSource Source);

/// <summary>
/// Where a <see cref="MidiTimeCodeUpdate"/> came from.
/// </summary>
[Experimental(MidiTimeCodeReader.DiagnosticId)]
public enum MidiTimeCodeSource
{
    /// <summary>An MTC Full Message (a jump to a new time, e.g. when locating).</summary>
    FullMessage,

    /// <summary>Eight Quarter Frames running forward; the time already includes the two-frame offset.</summary>
    QuarterFramesForward,

    /// <summary>Eight Quarter Frames running in reverse.</summary>
    QuarterFramesReverse,
}
