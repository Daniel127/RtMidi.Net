namespace RtMidi.Net;

/// <summary>
/// Which part of a parameter's value Data Increment (Control Change 96) and Data Decrement (97) change by one step,
/// according to the Recommended Practice RP-018 "Response to Data Inc/Dec Controllers".
/// </summary>
public enum MidiParameterStepUnit : byte
{
    /// <summary>
    /// The LSB, carrying into the MSB: Pitch Bend Sensitivity and Channel Fine Tuning (RPN 0 and 1), and any registered
    /// parameter the specification does not say otherwise for. For Pitch Bend Sensitivity, one step is one cent.
    /// </summary>
    Lsb,
    /// <summary>The MSB: Channel Coarse Tuning, Tuning Program Change and Tuning Bank Select (RPN 2, 3 and 4).</summary>
    Msb,
    /// <summary>Non-registered parameters: the manufacturer defines the step of each one.</summary>
    ManufacturerDefined,
}
