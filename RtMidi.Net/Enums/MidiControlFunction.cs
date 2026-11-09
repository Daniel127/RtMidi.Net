namespace RtMidi.Net;

/// <summary>
/// The function of a Control Change message: its control number (the first data byte), named as in the
/// MIDI 1.0 specification table "Control Changes and Mode Changes" (https://midi.org/midi-1-0-control-change-messages).
/// </summary>
/// <remarks>
/// <para>
/// The numbers the specification leaves undefined (3, 9, 14-15, 20-31, 35, 41, 46-47, 52-63, 85-87, 89-90 and 102-119)
/// have no name, but any number from 0 to 127 is valid: cast it, e.g. <c>(MidiControlFunction)20</c>.
/// Devices and DAWs often use the undefined and general purpose controllers for their own functions.
/// </para>
/// <para>
/// Controllers 0 to 31 are the most significant 7 bits (MSB) of a value; 32 to 63 are the least significant
/// bits (LSB) of the same controllers, for 14-bit resolution. 120 to 127 are Channel Mode Messages.
/// </para>
/// </remarks>
public enum MidiControlFunction : byte
{
    /// <summary>Bank Select (MSB).</summary>
    BankSelect = 0,
    /// <summary>Modulation Wheel or Lever (MSB).</summary>
    ModulationWheel = 1,
    /// <summary>Breath Controller (MSB).</summary>
    BreathController = 2,
    /// <summary>Foot Controller (MSB).</summary>
    FootController = 4,
    /// <summary>Portamento Time (MSB).</summary>
    PortamentoTime = 5,
    /// <summary>Data Entry (MSB): the value of the selected RPN or NRPN.</summary>
    DataEntry = 6,
    /// <summary>Channel Volume, formerly Main Volume (MSB).</summary>
    ChannelVolume = 7,
    /// <summary>Balance (MSB).</summary>
    Balance = 8,
    /// <summary>Pan (MSB).</summary>
    Pan = 10,
    /// <summary>Expression Controller (MSB).</summary>
    Expression = 11,
    /// <summary>Effect Control 1 (MSB).</summary>
    EffectControl1 = 12,
    /// <summary>Effect Control 2 (MSB).</summary>
    EffectControl2 = 13,
    /// <summary>General Purpose Controller 1 (MSB).</summary>
    GeneralPurpose1 = 16,
    /// <summary>General Purpose Controller 2 (MSB).</summary>
    GeneralPurpose2 = 17,
    /// <summary>General Purpose Controller 3 (MSB).</summary>
    GeneralPurpose3 = 18,
    /// <summary>General Purpose Controller 4 (MSB).</summary>
    GeneralPurpose4 = 19,
    /// <summary>LSB for <see cref="BankSelect"/>.</summary>
    BankSelectLsb = 32,
    /// <summary>LSB for <see cref="ModulationWheel"/>.</summary>
    ModulationWheelLsb = 33,
    /// <summary>LSB for <see cref="BreathController"/>.</summary>
    BreathControllerLsb = 34,
    /// <summary>LSB for <see cref="FootController"/>.</summary>
    FootControllerLsb = 36,
    /// <summary>LSB for <see cref="PortamentoTime"/>.</summary>
    PortamentoTimeLsb = 37,
    /// <summary>LSB for <see cref="DataEntry"/>.</summary>
    DataEntryLsb = 38,
    /// <summary>LSB for <see cref="ChannelVolume"/>.</summary>
    ChannelVolumeLsb = 39,
    /// <summary>LSB for <see cref="Balance"/>.</summary>
    BalanceLsb = 40,
    /// <summary>LSB for <see cref="Pan"/>.</summary>
    PanLsb = 42,
    /// <summary>LSB for <see cref="Expression"/>.</summary>
    ExpressionLsb = 43,
    /// <summary>LSB for <see cref="EffectControl1"/>.</summary>
    EffectControl1Lsb = 44,
    /// <summary>LSB for <see cref="EffectControl2"/>.</summary>
    EffectControl2Lsb = 45,
    /// <summary>LSB for <see cref="GeneralPurpose1"/>.</summary>
    GeneralPurpose1Lsb = 48,
    /// <summary>LSB for <see cref="GeneralPurpose2"/>.</summary>
    GeneralPurpose2Lsb = 49,
    /// <summary>LSB for <see cref="GeneralPurpose3"/>.</summary>
    GeneralPurpose3Lsb = 50,
    /// <summary>LSB for <see cref="GeneralPurpose4"/>.</summary>
    GeneralPurpose4Lsb = 51,
    /// <summary>Damper Pedal (Sustain): 63 or less is off, 64 or more is on.</summary>
    DamperPedal = 64,
    /// <summary>Portamento: 63 or less is off, 64 or more is on.</summary>
    Portamento = 65,
    /// <summary>Sostenuto: 63 or less is off, 64 or more is on.</summary>
    Sostenuto = 66,
    /// <summary>Soft Pedal: 63 or less is off, 64 or more is on.</summary>
    SoftPedal = 67,
    /// <summary>Legato Footswitch: 63 or less is normal, 64 or more is legato.</summary>
    LegatoFootswitch = 68,
    /// <summary>Hold 2: 63 or less is off, 64 or more is on.</summary>
    Hold2 = 69,
    /// <summary>Sound Controller 1 (default: Sound Variation).</summary>
    SoundController1 = 70,
    /// <summary>Sound Controller 2 (default: Timbre/Harmonic Intensity).</summary>
    SoundController2 = 71,
    /// <summary>Sound Controller 3 (default: Release Time).</summary>
    SoundController3 = 72,
    /// <summary>Sound Controller 4 (default: Attack Time).</summary>
    SoundController4 = 73,
    /// <summary>Sound Controller 5 (default: Brightness).</summary>
    SoundController5 = 74,
    /// <summary>Sound Controller 6 (default: Decay Time).</summary>
    SoundController6 = 75,
    /// <summary>Sound Controller 7 (default: Vibrato Rate).</summary>
    SoundController7 = 76,
    /// <summary>Sound Controller 8 (default: Vibrato Depth).</summary>
    SoundController8 = 77,
    /// <summary>Sound Controller 9 (default: Vibrato Delay).</summary>
    SoundController9 = 78,
    /// <summary>Sound Controller 10 (no default).</summary>
    SoundController10 = 79,
    /// <summary>General Purpose Controller 5.</summary>
    GeneralPurpose5 = 80,
    /// <summary>General Purpose Controller 6.</summary>
    GeneralPurpose6 = 81,
    /// <summary>General Purpose Controller 7.</summary>
    GeneralPurpose7 = 82,
    /// <summary>General Purpose Controller 8.</summary>
    GeneralPurpose8 = 83,
    /// <summary>Portamento Control.</summary>
    PortamentoControl = 84,
    /// <summary>High Resolution Velocity Prefix.</summary>
    HighResolutionVelocityPrefix = 88,
    /// <summary>Effects 1 Depth (default: Reverb Send Level).</summary>
    Effects1Depth = 91,
    /// <summary>Effects 2 Depth (formerly Tremolo Depth).</summary>
    Effects2Depth = 92,
    /// <summary>Effects 3 Depth (default: Chorus Send Level).</summary>
    Effects3Depth = 93,
    /// <summary>Effects 4 Depth (formerly Celeste/Detune Depth).</summary>
    Effects4Depth = 94,
    /// <summary>Effects 5 Depth (formerly Phaser Depth).</summary>
    Effects5Depth = 95,
    /// <summary>Data Increment (Data Entry +1).</summary>
    DataIncrement = 96,
    /// <summary>Data Decrement (Data Entry -1).</summary>
    DataDecrement = 97,
    /// <summary>Non-Registered Parameter Number (NRPN), LSB.</summary>
    NonRegisteredParameterNumberLsb = 98,
    /// <summary>Non-Registered Parameter Number (NRPN), MSB.</summary>
    NonRegisteredParameterNumberMsb = 99,
    /// <summary>Registered Parameter Number (RPN), LSB.</summary>
    RegisteredParameterNumberLsb = 100,
    /// <summary>Registered Parameter Number (RPN), MSB.</summary>
    RegisteredParameterNumberMsb = 101,
    /// <summary>Channel Mode Message: All Sound Off (value 0).</summary>
    AllSoundOff = 120,
    /// <summary>Channel Mode Message: Reset All Controllers (value 0).</summary>
    ResetAllControllers = 121,
    /// <summary>Channel Mode Message: Local Control (0 off, 127 on).</summary>
    LocalControl = 122,
    /// <summary>Channel Mode Message: All Notes Off (value 0).</summary>
    AllNotesOff = 123,
    /// <summary>Channel Mode Message: Omni Mode Off, plus all notes off (value 0).</summary>
    OmniModeOff = 124,
    /// <summary>Channel Mode Message: Omni Mode On, plus all notes off (value 0).</summary>
    OmniModeOn = 125,
    /// <summary>Channel Mode Message: Mono Mode On, plus poly off and all notes off (value: number of channels, or 0).</summary>
    MonoModeOn = 126,
    /// <summary>Channel Mode Message: Poly Mode On, plus mono off and all notes off (value 0).</summary>
    PolyModeOn = 127,
}
