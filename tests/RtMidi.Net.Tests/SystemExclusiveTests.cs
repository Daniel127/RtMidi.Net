namespace RtMidi.Net.Tests;

public class SystemExclusiveTests
{
    [Theory]
    [InlineData((byte)0x00)]
    [InlineData((byte)0x80)]
    public void ManufacturerId_InvalidOneByteId_Throws(byte id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MidiManufacturerId(id));
    }

    [Theory]
    [InlineData((byte)0x80, (byte)0x01)]
    [InlineData((byte)0x01, (byte)0x80)]
    [InlineData((byte)0x00, (byte)0x00)]
    public void ManufacturerId_InvalidExtendedId_Throws(byte second, byte third)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiManufacturerId.Extended(second, third));
    }

    [Fact]
    public void ManufacturerId_OneByte()
    {
        var id = new MidiManufacturerId(0x41);
        Assert.False(id.IsExtended);
        Assert.False(id.IsUniversal);
        Assert.Equal(1, id.Length);
        Assert.Equal(new byte[] { 0x41 }, id.ToArray());
        Assert.Equal("41H", id.ToString());
        Assert.Equal(new MidiManufacturerId(0x41), id);
        Assert.NotEqual(new MidiManufacturerId(0x43), id);
    }

    [Fact]
    public void ManufacturerId_Extended()
    {
        var id = MidiManufacturerId.Extended(0x20, 0x29);
        Assert.True(id.IsExtended);
        Assert.False(id.IsUniversal);
        Assert.Equal(3, id.Length);
        Assert.Equal(new byte[] { 0x00, 0x20, 0x29 }, id.ToArray());
        Assert.Equal("00H 20H 29H", id.ToString());
        Assert.Equal(MidiManufacturerId.Extended(0x20, 0x29), id);
        Assert.NotEqual(MidiManufacturerId.Extended(0x20, 0x2A), id);
    }

    [Fact]
    public void ManufacturerId_SpecialIds()
    {
        Assert.Equal(new MidiManufacturerId(0x7D), MidiManufacturerId.NonCommercial);
        Assert.False(MidiManufacturerId.NonCommercial.IsUniversal);
        Assert.True(MidiManufacturerId.UniversalNonRealTime.IsUniversal);
        Assert.True(MidiManufacturerId.UniversalRealTime.IsUniversal);
        Assert.Equal("7EH", MidiManufacturerId.UniversalNonRealTime.ToString());
        Assert.Equal("7FH", MidiManufacturerId.UniversalRealTime.ToString());
    }

    [Fact]
    public void ManufacturerId_ReadFromMessage()
    {
        Assert.Equal(new MidiManufacturerId(0x43), new MidiMessageSystemExclusive([0xF0, 0x43, 0x10, 0xF7]).ManufacturerId);
        Assert.Equal(MidiManufacturerId.Extended(0x20, 0x29),
            new MidiMessageSystemExclusive([0xF0, 0x00, 0x20, 0x29, 0x01, 0xF7]).ManufacturerId);
        Assert.Equal(MidiManufacturerId.UniversalNonRealTime, MidiMessageSystemExclusive.IdentityRequest().ManufacturerId);
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(new byte[] { 0xF0 })]
    [InlineData(new byte[] { 0x43, 0x10, 0xF7 })]
    [InlineData(new byte[] { 0xF0, 0xF7 })]
    [InlineData(new byte[] { 0xF0, 0x00, 0x20, 0xF7 })]
    [InlineData(new byte[] { 0xF0, 0x00, 0x00, 0x00, 0xF7 })]
    [InlineData(new byte[] { 0xF0, 0x00, 0x20, 0x80, 0xF7 })]
    [InlineData(new byte[] { 0xF0, 0x00, 0x80, 0x20, 0xF7 })]
    [InlineData(new byte[] { 0xF0, 0x00, 0xF7 })]
    public void ManufacturerId_Malformed_IsNull(byte[] data)
    {
        Assert.Null(new MidiMessageSystemExclusive(data).ManufacturerId);
    }

    [Fact]
    public void UniversalHeader_IsRead()
    {
        var message = new MidiMessageSystemExclusive([0xF0, 0x7E, 0x10, 0x06, 0x01, 0xF7]);
        Assert.True(message.IsUniversal);
        Assert.False(message.IsUniversalRealTime);
        Assert.Equal((byte)0x10, message.DeviceId);
        Assert.Equal((byte)0x06, message.SubId1);
        Assert.Equal((byte)0x01, message.SubId2);

        var realTime = new MidiTimeCode(0, 0, 0, 0, MidiTimeCodeRate.Fps24).ToFullMessage();
        Assert.True(realTime.IsUniversal);
        Assert.True(realTime.IsUniversalRealTime);
    }

    [Theory]
    [InlineData(new byte[] { 0xF0, 0x43, 0x10, 0x4C, 0x00, 0xF7 })] // manufacturer specific
    [InlineData(new byte[] { 0xF0, 0x7E, 0x10, 0xF7 })]             // too short for sub-IDs
    [InlineData(new byte[] { 0xF0, 0x7E })]
    [InlineData(new byte[] { })]
    public void UniversalHeader_MissingParts_AreNull(byte[] data)
    {
        var message = new MidiMessageSystemExclusive(data);
        Assert.Null(message.SubId1);
        Assert.Null(message.SubId2);
        if (!message.IsUniversal) Assert.Null(message.DeviceId);
    }

    [Fact]
    public void Factories_CreateUniversalMessages()
    {
        Assert.Equal(new byte[] { 0xF0, 0x7E, 0x7F, 0x06, 0x01, 0xF7 }, MidiMessageSystemExclusive.IdentityRequest().Data);
        Assert.Equal(new byte[] { 0xF0, 0x7E, 0x10, 0x06, 0x01, 0xF7 }, MidiMessageSystemExclusive.IdentityRequest(0x10).Data);
        Assert.Equal(new byte[] { 0xF0, 0x7E, 0x7F, 0x09, 0x01, 0xF7 }, MidiMessageSystemExclusive.GeneralMidiSystemOn().Data);
        Assert.Equal(new byte[] { 0xF0, 0x7E, 0x00, 0x09, 0x01, 0xF7 }, MidiMessageSystemExclusive.GeneralMidiSystemOn(0).Data);
        Assert.Equal(new byte[] { 0xF0, 0x7E, 0x7F, 0x09, 0x02, 0xF7 }, MidiMessageSystemExclusive.GeneralMidiSystemOff().Data);
        Assert.Equal(new byte[] { 0xF0, 0x7E, 0x05, 0x09, 0x02, 0xF7 }, MidiMessageSystemExclusive.GeneralMidiSystemOff(5).Data);
    }

    [Fact]
    public void Factories_InvalidDeviceId_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiMessageSystemExclusive.IdentityRequest(0x80));
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiMessageSystemExclusive.GeneralMidiSystemOn(0x80));
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiMessageSystemExclusive.GeneralMidiSystemOff(0x80));
    }

    [Fact]
    public void Factories_RoundTripThroughParse()
    {
        var message = MidiMessageSystemExclusive.GeneralMidiSystemOn();
        Assert.Equal(message, MidiMessageConverter.Parse(MidiMessageConverter.ToBytes(message)));
    }

    [Fact]
    public void TryGetIdentityReply_OneByteManufacturer()
    {
        var message = new MidiMessageSystemExclusive(
            [0xF0, 0x7E, 0x10, 0x06, 0x02, 0x43, 0x01, 0x02, 0x7F, 0x01, 0x00, 0x01, 0x02, 0x03, 0xF7]);

        Assert.True(message.TryGetIdentityReply(out var reply));

        Assert.Equal(new MidiIdentityReply(0x10, new MidiManufacturerId(0x43), 0x01 | (0x02 << 7), 0x7F | (0x01 << 7), 0x00010203u),
            reply);
    }

    [Fact]
    public void TryGetIdentityReply_ExtendedManufacturer()
    {
        var message = new MidiMessageSystemExclusive(
            [0xF0, 0x7E, 0x7F, 0x06, 0x02, 0x00, 0x20, 0x29, 0x7F, 0x7F, 0x00, 0x00, 0x7F, 0x7F, 0x7F, 0x7F, 0xF7]);

        Assert.True(message.TryGetIdentityReply(out var reply));

        Assert.Equal(MidiManufacturerId.Extended(0x20, 0x29), reply.Manufacturer);
        Assert.Equal(16383, reply.DeviceFamily);
        Assert.Equal(0, reply.DeviceFamilyMember);
        Assert.Equal(0x7F7F7F7Fu, reply.SoftwareRevision);
        Assert.Equal((byte)0x7F, reply.DeviceId);
    }

    [Theory]
    [InlineData(new byte[] { 0xF0, 0x7E, 0x7F, 0x06, 0x01, 0xF7 })]                                                    // request
    [InlineData(new byte[] { 0xF0, 0x7F, 0x10, 0x06, 0x02, 0x43, 0x01, 0x02, 0x7F, 0x01, 0x00, 0x01, 0x02, 0x03, 0xF7 })] // real time
    [InlineData(new byte[] { 0xF0, 0x7E, 0x10, 0x06, 0x02, 0x43, 0x01, 0x02, 0x7F, 0x01, 0x00, 0x01, 0x02, 0xF7 })]       // short
    [InlineData(new byte[] { 0xF0, 0x7E, 0x10, 0x06, 0x02, 0x43, 0x01, 0x02, 0x7F, 0x01, 0x00, 0x01, 0x02, 0x03, 0x04, 0xF7 })] // long
    [InlineData(new byte[] { 0xF0, 0x7E, 0x10, 0x06, 0x02, 0x43, 0x01, 0x02, 0x7F, 0x01, 0x00, 0x01, 0x02, 0x03, 0x00 })] // no F7
    [InlineData(new byte[] { 0xF0, 0x7E, 0x10, 0x06, 0x02, 0x43, 0x81, 0x02, 0x7F, 0x01, 0x00, 0x01, 0x02, 0x03, 0xF7 })] // not 7-bit
    [InlineData(new byte[] { 0xF0, 0x7E, 0x10, 0x06, 0x02, 0x00, 0x00, 0x00, 0x01, 0x02, 0x7F, 0x01, 0x00, 0x01, 0x02, 0x03, 0xF7 })] // bad ID
    [InlineData(new byte[] { 0xF0, 0x7E, 0x10, 0x06, 0x02, 0x00, 0x20, 0xF7 })]                                        // truncated ID
    [InlineData(new byte[] { 0xF0, 0x7E, 0x80, 0x06, 0x02, 0x43, 0x01, 0x02, 0x7F, 0x01, 0x00, 0x01, 0x02, 0x03, 0xF7 })] // device ID
    public void TryGetIdentityReply_RejectsOtherMessages(byte[] data)
    {
        Assert.False(new MidiMessageSystemExclusive(data).TryGetIdentityReply(out var reply));
        Assert.Null(reply);
    }

    [Fact]
    public void Equality_ComparesBytes()
    {
        var a = new MidiMessageSystemExclusive([0xF0, 0x43, 0x10, 0xF7]);
        var b = new MidiMessageSystemExclusive([0xF0, 0x43, 0x10, 0xF7]);
        var c = new MidiMessageSystemExclusive([0xF0, 0x43, 0x11, 0xF7]);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.True(a == b);
        Assert.NotEqual(a, c);
        Assert.False(a.Equals(null));
        Assert.False(a.Equals((object)new MidiMessageUnknown([0xF0, 0x43, 0x10, 0xF7])));
    }

    [Fact]
    public void Unknown_Equality_ComparesBytes()
    {
        var a = new MidiMessageUnknown([0xF4, 0x01]);
        var b = new MidiMessageUnknown([0xF4, 0x01]);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.NotEqual(a, new MidiMessageUnknown([0xF4, 0x02]));
        Assert.False(a.Equals(null));
    }
}
