using RtMidi.Net.Enums;
using RtMidi.Net.Events;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net;

public class MidiInputClient : MidiClient
{
    private readonly RtMidiIn _rtMidiInClient;
    private bool _ignoreSysexMessages = true;
    private bool _ignoreTimeMessages = true;
    private bool _ignoreSenseMessages = true;

    public bool IgnoreSysexMessages
    {
        get => _ignoreSysexMessages;
        set
        {
            _ignoreSysexMessages = value;
            IgnoreMessageTypes(_ignoreSysexMessages, _ignoreTimeMessages, _ignoreSenseMessages);
        }
    }

    public bool IgnoreTimeMessages
    {
        get => _ignoreTimeMessages;
        set
        {
            _ignoreTimeMessages = value;
            IgnoreMessageTypes(_ignoreSysexMessages, _ignoreTimeMessages, _ignoreSenseMessages);
        }
    }

    public bool IgnoreSenseMessages
    {
        get => _ignoreSenseMessages;
        set
        {
            _ignoreSenseMessages = value;
            IgnoreMessageTypes(_ignoreSysexMessages, _ignoreTimeMessages, _ignoreSenseMessages);
        }
    }

    public event EventHandler<MidiMessageReceivedEventArgs>? OnMessageReceived;

    public MidiInputClient(MidiDeviceInfo deviceInfo) : base(deviceInfo, new RtMidiIn())
    {
        _rtMidiInClient = (RtMidiIn) RtMidiClient;
    }

    public void ActivateMessageReceivedEvent()
    {
        _rtMidiInClient.SetCallback(InternalMessageReceived, null);
    }

    public void DeactivateMessageReceivedEvent()
    {
        _rtMidiInClient.CancelCallback();
    }

    private void IgnoreMessageTypes(bool sysex, bool time, bool sense)
    {
        _rtMidiInClient.IgnoreTypes(sysex, time, sense);
    }

    private void InternalMessageReceived(double timestamp, IntPtr messagePtr, UIntPtr messageSize, IntPtr userDataPtr)
    {
        var messageData = new byte [(int) messageSize];
        System.Runtime.InteropServices.Marshal.Copy(messagePtr, messageData, 0, (int) messageSize);
        var message = ConvertMessage(messageData);
        var midiEventArgs = new MidiMessageReceivedEventArgs(message, TimeSpan.FromSeconds(timestamp));
        OnMessageReceived?.Invoke(this, midiEventArgs);
    }

    public (MidiMessage, TimeSpan) GetMessage()
    {
        var (messageData, timestamp) = _rtMidiInClient.GetMessage();
        return (ConvertMessage(messageData), TimeSpan.FromSeconds(timestamp));
    }

    private static MidiMessage ConvertMessage(IReadOnlyList<byte> message)
    {
        MidiMessageType type;

        if (message[0] is >= (byte) MidiMessageType.NoteOff and < (byte) MidiMessageType.SystemExclusive)
        {
            var channel = (MidiChannel) (0b_0000_1111 & message[0]);
            type = (MidiMessageType) (0b_1111_0000 & message[0]);

            switch (type)
            {
                case MidiMessageType.NoteOff:
                case MidiMessageType.NoteOn:
                    return new MidiMessageNote(channel, new MidiNote(message[1]), message[2], type);
                case MidiMessageType.PolyphonicKeyPressure:
                    return new MidiMessageNoteAfterTouch(channel, new MidiNote(message[1]), message[2]);
                case MidiMessageType.ProgramChange:
                    return new MidiMessageProgramChange(channel, message[1]);
                case MidiMessageType.ChannelPressure:
                    return new MidiMessageChannelAfterTouch(channel, message[1]);
                case MidiMessageType.PitchBendChange:
                    return new MidiMessagePitchBendChange(channel, message[1], message[2]);
                case MidiMessageType.ControlChange:
                    return new MidiMessageControlChange(channel, message[1], message[2]);
                default:
                    throw new Exception("Type is out of range");
            }
        }

        if (!Enum.IsDefined(typeof(MidiMessageType), message[0]))
        {
            return new MidiMessageUnknown(message.ToArray());
        }

        type = (MidiMessageType) message[0];
        switch (type)
        {
            case MidiMessageType.SystemExclusive:
                return new MidiMessageSystemExclusive(message.ToArray());
            case MidiMessageType.SongPositionPointer:
                return new MidiMessageSongPositionPointer(message[1], message[2]);
            case MidiMessageType.SongSelect:
                return new MidiMessageSongSelect(message[1]);
            case MidiMessageType.TimeCodeQuarterFrame:
                throw new NotImplementedException();
            default:
                return new MidiMessage(type);
        }
    }
}