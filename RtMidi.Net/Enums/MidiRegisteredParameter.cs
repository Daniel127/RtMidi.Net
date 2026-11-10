namespace RtMidi.Net;

/// <summary>
/// The Registered Parameter Numbers (RPN) of the MIDI 1.0 specification, table "Registered Parameter Numbers"
/// (https://midi.org/midi-1-0-control-change-messages). The value is the 14-bit parameter number,
/// <c>(MSB &lt;&lt; 7) | LSB</c>, as selected with Control Changes 101 (MSB) and 100 (LSB).
/// </summary>
/// <remarks>
/// The numbers the specification does not list are reserved; any value from 0 to 16383 is valid with a cast.
/// The value of a parameter is set with Data Entry (Control Changes 6 and 38); see <see cref="MidiParameterMessages"/>.
/// </remarks>
public enum MidiRegisteredParameter : ushort
{
    /// <summary>
    /// 00H 00H, Pitch Bend Sensitivity: the range of the pitch bend wheel. Data Entry MSB = ± semitones,
    /// LSB = ± cents.
    /// </summary>
    PitchBendSensitivity = 0x0000,
    /// <summary>
    /// 00H 01H, Channel Fine Tuning (formerly Fine Tuning): resolution 100/8192 cents; 00H 00H = -100 cents,
    /// 40H 00H = A440, 7FH 7FH = +100 cents.
    /// </summary>
    ChannelFineTuning = 0x0001,
    /// <summary>
    /// 00H 02H, Channel Coarse Tuning (formerly Coarse Tuning): only the Data Entry MSB is used, in semitones;
    /// 00H = -64, 40H = A440, 7FH = +63.
    /// </summary>
    ChannelCoarseTuning = 0x0002,
    /// <summary>00H 03H, Tuning Program Change: the Data Entry value is the tuning program number.</summary>
    TuningProgramChange = 0x0003,
    /// <summary>00H 04H, Tuning Bank Select: the Data Entry value is the tuning bank number.</summary>
    TuningBankSelect = 0x0004,
    /// <summary>00H 05H, Modulation Depth Range (General MIDI 2).</summary>
    ModulationDepthRange = 0x0005,
    /// <summary>00H 06H, MPE Configuration Message (MIDI Polyphonic Expression).</summary>
    MpeConfiguration = 0x0006,
    /// <summary>3DH 00H, Three Dimensional Sound Controllers: Azimuth Angle (RP-049).</summary>
    Sound3DAzimuthAngle = 0x3D << 7,
    /// <summary>3DH 01H, Three Dimensional Sound Controllers: Elevation Angle (RP-049).</summary>
    Sound3DElevationAngle = (0x3D << 7) | 0x01,
    /// <summary>3DH 02H, Three Dimensional Sound Controllers: Gain (RP-049).</summary>
    Sound3DGain = (0x3D << 7) | 0x02,
    /// <summary>3DH 03H, Three Dimensional Sound Controllers: Distance Ratio (RP-049).</summary>
    Sound3DDistanceRatio = (0x3D << 7) | 0x03,
    /// <summary>3DH 04H, Three Dimensional Sound Controllers: Maximum Distance (RP-049).</summary>
    Sound3DMaximumDistance = (0x3D << 7) | 0x04,
    /// <summary>3DH 05H, Three Dimensional Sound Controllers: Gain at Maximum Distance (RP-049).</summary>
    Sound3DGainAtMaximumDistance = (0x3D << 7) | 0x05,
    /// <summary>3DH 06H, Three Dimensional Sound Controllers: Reference Distance Ratio (RP-049).</summary>
    Sound3DReferenceDistanceRatio = (0x3D << 7) | 0x06,
    /// <summary>3DH 07H, Three Dimensional Sound Controllers: Pan Spread Angle (RP-049).</summary>
    Sound3DPanSpreadAngle = (0x3D << 7) | 0x07,
    /// <summary>3DH 08H, Three Dimensional Sound Controllers: Roll Angle (RP-049).</summary>
    Sound3DRollAngle = (0x3D << 7) | 0x08,
    /// <summary>
    /// 7FH 7FH, the Null Function Number for RPN and NRPN: selecting it disables Data Entry, Data Increment and
    /// Data Decrement until a new RPN or NRPN is selected.
    /// </summary>
    Null = 0x3FFF,
}
