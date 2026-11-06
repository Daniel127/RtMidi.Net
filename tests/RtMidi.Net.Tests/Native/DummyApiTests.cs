using RtMidi.Net.Clients;
using RtMidi.Net.Enums;
using RtMidi.Net.Events;
using RtMidi.Net.InteropServices;
using System.Runtime.InteropServices;

namespace RtMidi.Net.Tests.Native;

/// <summary>
/// End-to-end tests of the interop layer against RtMidi built with only its dummy
/// API: every call works but there are no ports and no messages.
/// </summary>
public class DummyApiTests
{
    [NativeFact]
    public void Version_IsRtMidi6()
    {
        Assert.StartsWith("6.", MidiManager.GetRtMidiVersion());
    }

    [NativeFact]
    public void AvailableApis_ContainsTheDummyApi()
    {
        var apis = MidiManager.GetAvailableApis();

        Assert.Contains(MidiApi.RtMidiDummy, apis);
        Assert.All(apis, api => Assert.True(Enum.IsDefined(api)));
    }

    [NativeFact]
    public void ApiNames()
    {
        Assert.Equal("dummy", MidiManager.GetApiName(MidiApi.RtMidiDummy));
        Assert.Equal("Dummy", MidiManager.GetApiDisplayName(MidiApi.RtMidiDummy));
        Assert.Equal("alsa", MidiManager.GetApiName(MidiApi.LinuxAlsa));
        Assert.Equal(string.Empty, MidiManager.GetApiName((MidiApi)200));
        Assert.Equal("Unknown", MidiManager.GetApiDisplayName((MidiApi)200));
    }

    [NativeFact]
    public void CompiledApiByName()
    {
        Assert.Equal(MidiApi.RtMidiDummy, RtMidiBase.GetCompiledApiByName("dummy"));
        Assert.Equal(MidiApi.Unspecified, RtMidiBase.GetCompiledApiByName("no such api"));
    }

    [NativeFact]
    public void Devices_AreEmpty()
    {
        Assert.Empty(MidiManager.GetAvailableDevices());
        Assert.Empty(MidiManager.GetAvailableDevices(MidiApi.RtMidiDummy));
    }

    [NativeFact]
    public void GetDeviceInfo_UnknownPort_Throws()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => MidiManager.GetDeviceInfo(0, MidiDeviceType.Input));
        Assert.Equal("deviceId", exception.ParamName);
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiManager.GetDeviceInfo(3, MidiDeviceType.Output, MidiApi.RtMidiDummy));
    }

    [NativeFact]
    public void RtMidiIn_PortsAndApi()
    {
        using var rtMidiIn = new RtMidiIn(MidiApi.RtMidiDummy, "Tests", 10);

        Assert.Equal(MidiApi.RtMidiDummy, rtMidiIn.GetCurrentApi());
        Assert.Equal(0u, rtMidiIn.GetPortCount());
        Assert.Equal(string.Empty, rtMidiIn.GetPortName(0));
        Assert.False(rtMidiIn.IsPortOpen);

        rtMidiIn.OpenPort(0);
        Assert.True(rtMidiIn.IsPortOpen);
        rtMidiIn.ClosePort();
        Assert.False(rtMidiIn.IsPortOpen);

        rtMidiIn.OpenPort(0, "Named port");
        rtMidiIn.OpenVirtualPort("Virtual");
        rtMidiIn.IgnoreTypes(false, false, false);
    }

    [NativeFact]
    public void RtMidiIn_GetMessage_EmptyQueue()
    {
        using var rtMidiIn = new RtMidiIn();

        var (message, _) = rtMidiIn.GetMessage();

        Assert.Empty(message);
    }

    [NativeFact]
    public void RtMidiOut_SendMessage()
    {
        using var rtMidiOut = new RtMidiOut(MidiApi.RtMidiDummy, "Tests");

        Assert.Equal(MidiApi.RtMidiDummy, rtMidiOut.GetCurrentApi());
        rtMidiOut.OpenVirtualPort("Out");
        rtMidiOut.SendMessage([0x90, 60, 100]);
        Assert.Throws<ArgumentException>(() => rtMidiOut.SendMessage([]));
    }

    [NativeFact]
    public void Dispose_IsIdempotent()
    {
        // Disposing twice is exactly what this test checks.
#pragma warning disable S3966 // Objects should not be disposed more than once
        var rtMidiIn = new RtMidiIn();
        rtMidiIn.Dispose();
        rtMidiIn.Dispose();

        var client = new MidiOutputClient();
        client.Dispose();
        client.Dispose();
#pragma warning restore S3966
    }

    [NativeFact]
    public void InputClient_WithoutDevice_CannotOpen()
    {
        using var client = new MidiInputClient();

        Assert.Throws<InvalidOperationException>(client.Open);
    }

    [NativeFact]
    public void InputClient_VirtualPort_AndIgnoredTypes()
    {
        using var client = new MidiInputClient();
        client.OpenVirtual("Tests in");

        client.IgnoreSysexMessages = false;
        client.IgnoreTimeMessages = false;
        client.IgnoreSenseMessages = false;

        Assert.False(client.IgnoreSysexMessages);
        Assert.False(client.IgnoreTimeMessages);
        Assert.False(client.IgnoreSenseMessages);
        Assert.False(client.TryGetMessage(out var message, out _));
        Assert.Null(message);
        client.Close();
    }

    [NativeFact]
    public void InputClient_ForDevice_OpensItOnce()
    {
        var device = new MidiDeviceInfo("Dummy", 0, MidiApi.RtMidiDummy, MidiDeviceType.Input);
        using var client = new MidiInputClient(device);

        client.Open();
        client.Open(); // already open: no-op
        client.Close();
    }

    [NativeFact]
    public async Task InputClient_GetMessageAsync_CanBeCancelled()
    {
        using var client = new MidiInputClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetMessageAsync(cts.Token));
    }

    [NativeFact]
    public void InputClient_Event_IsRaisedFromTheNativeCallback()
    {
        using var client = new MidiInputClient();
        var received = new List<MidiMessageReceivedEventArgs>();
        client.OnMessageReceived += (_, e) => received.Add(e);
        client.ActivateMessageReceivedEvent();

        // Call the callback the way RtMidi does, with the user data registered for this client.
        InvokeNativeCallback(0.5, [0x91, 64, 90], ((RtMidiIn)client.RtMidiClient).CallbackUserData);

        var args = Assert.Single(received);
        Assert.Equal(new MidiMessageNote(MidiChannel.Channel02, new MidiNote(64), 90, MidiMessageType.NoteOn), args.Message);
        Assert.Equal(TimeSpan.FromSeconds(0.5), args.Timestamp);

        client.DeactivateMessageReceivedEvent();
        Assert.Equal(IntPtr.Zero, ((RtMidiIn)client.RtMidiClient).CallbackUserData);
    }

    [NativeFact]
    public void InputClient_HandlerException_DoesNotEscapeToNativeCode()
    {
        using var client = new MidiInputClient();
        client.OnMessageReceived += (_, _) => throw new InvalidOperationException("handler failure");
        client.ActivateMessageReceivedEvent();

        // Would terminate the process if the exception crossed the native boundary.
        InvokeNativeCallback(0, [0xF8], ((RtMidiIn)client.RtMidiClient).CallbackUserData);
        InvokeNativeCallback(0, [0xF8], IntPtr.Zero);
    }

    [NativeFact]
    public void OutputClient_SendsEncodedMessages()
    {
        var device = new MidiDeviceInfo("Dummy", 0, MidiApi.RtMidiDummy, MidiDeviceType.Output);
        using var client = new MidiOutputClient(device);
        client.Open();

        client.SendMessage(new MidiMessageControlChange(MidiChannel.Channel01, 7, 100));
        client.SendMessage([0xF8]);
        Assert.Throws<ArgumentNullException>(() => client.SendMessage((byte[])null!));
        Assert.Throws<ArgumentException>(() => client.SendMessage(new MidiMessage(MidiMessageType.NoteOn)));
    }

    /// <summary>
    /// Calls the callback the way RtMidi does: with the message in native memory.
    /// </summary>
    private static void InvokeNativeCallback(double timestamp, byte[] message, IntPtr userData)
    {
        var buffer = Marshal.AllocHGlobal(message.Length);
        try
        {
            Marshal.Copy(message, 0, buffer, message.Length);
            RtMidiIn.OnNativeMessage(timestamp, buffer, (nuint)message.Length, userData);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
