namespace RtMidi.Net;

/// <summary>
/// The answer of a device to an Identity Request (<see cref="MidiMessageSystemExclusive.IdentityRequest()"/>): who
/// made it, which product it is and its software version. See
/// <see cref="MidiMessageSystemExclusive.TryGetIdentityReply"/>.
/// </summary>
/// <param name="DeviceId">The device ID of the device that answered.</param>
/// <param name="Manufacturer">The manufacturer's System Exclusive ID.</param>
/// <param name="DeviceFamily">The device family code, 14 bits (0 to 16383), defined by the manufacturer.</param>
/// <param name="DeviceFamilyMember">The device family member code, 14 bits (0 to 16383), defined by the manufacturer.</param>
/// <param name="SoftwareRevision">The four software revision bytes, in the order they are sent (the first one in the
/// highest byte). Their format is device specific.</param>
public sealed record MidiIdentityReply(byte DeviceId, MidiManufacturerId Manufacturer, int DeviceFamily, int DeviceFamilyMember,
    uint SoftwareRevision);
