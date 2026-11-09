namespace RtMidi.Net;

/// <summary>
/// Whether a parameter number is a Registered Parameter Number (RPN, defined by the MIDI specification and selected
/// with Control Changes 101/100) or a Non-Registered Parameter Number (NRPN, defined by each manufacturer and selected
/// with Control Changes 99/98).
/// </summary>
public enum MidiParameterKind : byte
{
    /// <summary>Registered Parameter Number (RPN), see <see cref="MidiRegisteredParameter"/>.</summary>
    Registered,
    /// <summary>Non-Registered Parameter Number (NRPN), defined by the manufacturer of the device.</summary>
    NonRegistered,
}
