namespace RtMidi.Net;

/// <summary>
/// The manufacturer ID at the start of a System Exclusive message: one byte (01H to 7FH) or three bytes starting with
/// 00H (e.g. 00H 20H 29H). The MIDI Association assigns them.
/// </summary>
/// <remarks>
/// 7DH is reserved for non-commercial use, and 7EH and 7FH are the Universal System Exclusive IDs (Non-Real Time and
/// Real Time). The default value (00H 00H 00H) is not a valid ID.
/// </remarks>
public readonly record struct MidiManufacturerId
{
    private readonly byte _first;
    private readonly byte _second;
    private readonly byte _third;

    /// <summary>
    /// A one-byte manufacturer ID.
    /// </summary>
    /// <param name="id">01H to 7FH.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is 0 or above 7FH.</exception>
    public MidiManufacturerId(byte id)
    {
        if (id is 0 or > 0x7F) throw new ArgumentOutOfRangeException(nameof(id), id, "A one-byte ID is 01H to 7FH.");
        _first = id;
    }

    private MidiManufacturerId(byte first, byte second, byte third)
    {
        _first = first;
        _second = second;
        _third = third;
    }

    /// <summary>The non-commercial ID, 7DH (schools, research; not for products).</summary>
    public static MidiManufacturerId NonCommercial { get; } = new(0x7D);

    /// <summary>The Universal Non-Real Time System Exclusive ID, 7EH.</summary>
    public static MidiManufacturerId UniversalNonRealTime { get; } = new(0x7E);

    /// <summary>The Universal Real Time System Exclusive ID, 7FH.</summary>
    public static MidiManufacturerId UniversalRealTime { get; } = new(0x7F);

    /// <summary>
    /// A three-byte manufacturer ID: 00H followed by two more bytes.
    /// </summary>
    /// <param name="second">The second byte, 00H to 7FH.</param>
    /// <param name="third">The third byte, 00H to 7FH.</param>
    /// <returns>The ID 00H <paramref name="second"/> <paramref name="third"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A byte is above 7FH, or both are 0 (00H 00H 00H is not used).</exception>
    public static MidiManufacturerId Extended(byte second, byte third)
    {
        if (second > 0x7F) throw new ArgumentOutOfRangeException(nameof(second), second, "A data byte is 00H to 7FH.");
        if (third > 0x7F) throw new ArgumentOutOfRangeException(nameof(third), third, "A data byte is 00H to 7FH.");
        if (second == 0 && third == 0) throw new ArgumentOutOfRangeException(nameof(third), third, "00H 00H 00H is not used.");
        return new MidiManufacturerId(0, second, third);
    }

    /// <summary>Whether it is a three-byte ID (starting with 00H).</summary>
    public bool IsExtended => _first == 0;

    /// <summary>Whether it is 7EH or 7FH, a Universal System Exclusive ID.</summary>
    public bool IsUniversal => _first is 0x7E or 0x7F;

    /// <summary>The number of bytes of the ID in a message: 1 or 3.</summary>
    public int Length => IsExtended ? 3 : 1;

    /// <summary>The bytes of the ID, as sent after F0H.</summary>
    /// <returns>One or three bytes.</returns>
    public byte[] ToArray() => IsExtended ? [0, _second, _third] : [_first];

    /// <summary>The ID in hexadecimal, e.g. <c>41H</c> or <c>00H 20H 29H</c>.</summary>
    /// <returns>The formatted ID.</returns>
    public override string ToString() => IsExtended ? $"00H {_second:X2}H {_third:X2}H" : $"{_first:X2}H";

    /// <summary>
    /// Reads an ID from the bytes that follow F0H. Returns false if they are too short or not a valid ID.
    /// </summary>
    internal static bool TryRead(ReadOnlySpan<byte> bytes, out MidiManufacturerId id)
    {
        id = default;
        if (bytes.IsEmpty || bytes[0] > 0x7F) return false;
        if (bytes[0] != 0)
        {
            id = new MidiManufacturerId(bytes[0]);
            return true;
        }

        if (bytes.Length < 3 || bytes[1] > 0x7F || bytes[2] > 0x7F || (bytes[1] == 0 && bytes[2] == 0)) return false;
        id = new MidiManufacturerId(0, bytes[1], bytes[2]);
        return true;
    }
}
