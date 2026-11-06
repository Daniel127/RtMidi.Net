using System.Runtime.CompilerServices;
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
    }

    [NativeFact]
    public void InputClient_LateCallback_AfterCancelOrDispose_IsIgnored()
    {
        var client = new MidiInputClient();
        var received = 0;
        client.OnMessageReceived += (_, _) => received++;
        client.ActivateMessageReceivedEvent();
        var userData = ((RtMidiIn)client.RtMidiClient).CallbackUserData;
        Assert.NotEqual(IntPtr.Zero, userData);

        // RtMidi 6.0 does not wait for a running callback when it is cancelled or freed.
        client.DeactivateMessageReceivedEvent();
        InvokeNativeCallback(0, [0xF8], userData);
        client.Dispose();
        InvokeNativeCallback(0, [0xF8], userData);

        Assert.Equal(0, received);
        Assert.Equal(IntPtr.Zero, ((RtMidiIn)client.RtMidiClient).CallbackUserData);
    }

    [NativeFact]
    public void RtMidiIn_WithCallback_IsNotKeptAliveByTheRegistration()
    {
        var reference = CreateInputWithCallback();

        for (var i = 0; i < 10 && reference.IsAlive; i++)
        {
#pragma warning disable S1215 // forcing a collection is what this test checks
            GC.Collect();
#pragma warning restore S1215
            GC.WaitForPendingFinalizers();
        }

        Assert.False(reference.IsAlive);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateInputWithCallback()
    {
        var input = new RtMidiIn(MidiApi.RtMidiDummy, "Leak test");
        input.SetCallback((_, _) => { });
        return new WeakReference(input);
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

    [NativeFact]
    public void GetMessage_CopiesQueuedMessages_AndSkipsOversizedOnes()
    {
        using var input = new RtMidiIn(MidiApi.RtMidiDummy, "Queue test");
        var exactSize = new byte[RtMidiIn.MessageBufferSize];
        exactSize[0] = 0xF0;
        exactSize[^1] = 0xF7;
        var queue = new FakeNativeQueue(
            (0.25, [0x90, 60, 100]),
            (0.5, exactSize),
            (1.0, new byte[RtMidiIn.MessageBufferSize + 1]),
            (2.0, [0xB0, 7, 64]));

        AssertNextMessage(input, queue, [0x90, 60, 100], 0.25);
        AssertNextMessage(input, queue, exactSize, 0.5);
        // Too big for the buffer: RtMidi removes it from the queue without copying it.
        AssertNextMessage(input, queue, [], 1.0);
        // The next message is read normally, with nothing left from the previous ones.
        AssertNextMessage(input, queue, [0xB0, 7, 64], 2.0);
        // Empty queue.
        AssertNextMessage(input, queue, [], 0.0);
    }

    private static void AssertNextMessage(RtMidiIn input, FakeNativeQueue queue, byte[] expectedMessage, double expectedTimestamp)
    {
        var (message, timestamp) = input.GetMessage(queue.Read);
        Assert.Equal(expectedMessage, message);
        Assert.Equal(expectedTimestamp, timestamp);
    }

    /// <summary>
    /// Behaves like <c>rtmidi_in_get_message</c> in RtMidi 6.0.
    /// </summary>
    private sealed class FakeNativeQueue(params (double Timestamp, byte[] Message)[] messages)
    {
        private readonly Queue<(double Timestamp, byte[] Message)> _messages = new(messages);

        public double Read(byte[] buffer, ref nuint size)
        {
            if (!_messages.TryDequeue(out var next))
            {
                size = 0;
                return 0;
            }

            if (next.Message.Length <= (int)size) next.Message.CopyTo(buffer, 0);
            size = (nuint)next.Message.Length;
            return next.Timestamp;
        }
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
