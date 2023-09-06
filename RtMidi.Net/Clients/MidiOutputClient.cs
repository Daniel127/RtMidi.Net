using RtMidi.Net.Enums;
using RtMidi.Net.InteropServices;

namespace RtMidi.Net.Clients;

public class MidiOutputClient : MidiClient
{
    private readonly RtMidiOut _rtMidiOutClient;

    public MidiOutputClient(MidiDeviceInfo deviceInfo) : base(deviceInfo, new RtMidiOut())
    {
        _rtMidiOutClient = (RtMidiOut)RtMidiClient;
    }

    public void SendMessage(byte[] message)
    {
        _rtMidiOutClient.SendMessage(message);
    }

    public void SendMessage(MidiMessage message)
    {
        var messageData = ConvertMessage(message);
        _rtMidiOutClient.SendMessage(messageData);
    }

    private static byte[] ConvertMessage(MidiMessage message)
    {
        byte[] messageData = message.Type switch
        {
            MidiMessageType.Unknown when message is MidiMessageUnknown messageUnknown => messageUnknown.MessageData,
            MidiMessageType.NoteOff or MidiMessageType.NoteOn when message is MidiMessageNote messageNote => new[]
            {
                ByteFrom(messageNote.Type, messageNote.Channel),
                messageNote.Note.GetByteRepresentation(),
                messageNote.Velocity
            },
            MidiMessageType.PolyphonicKeyPressure when message is MidiMessageNoteAfterTouch messageAfterTouch => new[]
            {
                ByteFrom(messageAfterTouch.Type, messageAfterTouch.Channel),
                messageAfterTouch.Note.GetByteRepresentation(),
                messageAfterTouch.Pressure
            },
            MidiMessageType.ControlChange when message is MidiMessageControlChange messageControlChange => new[]
            {
                ByteFrom(messageControlChange.Type, messageControlChange.Channel),
                messageControlChange.ControlFunction,
                messageControlChange.Value
            },
            MidiMessageType.ProgramChange when message is MidiMessageProgramChange messageProgramChange => new[]
            {
                ByteFrom(messageProgramChange.Type, messageProgramChange.Channel),
                messageProgramChange.Program
            },
            MidiMessageType.ChannelPressure when message is MidiMessageChannelAfterTouch messageChannelAfTouch => new[]
            {
                ByteFrom(messageChannelAfTouch.Type, messageChannelAfTouch.Channel),
                messageChannelAfTouch.Pressure
            },
            MidiMessageType.PitchBendChange when message is MidiMessagePitchBendChange messagePitchBendChange => new[]
            {
                ByteFrom(messagePitchBendChange.Type, messagePitchBendChange.Chanel),
                messagePitchBendChange.Lsb,
                messagePitchBendChange.Msb
            },
            MidiMessageType.SystemExclusive when message is MidiMessageSystemExclusive(var data, _) => data,
            //TODO Create Message for TimeCodeQuarterFrame
            MidiMessageType.TimeCodeQuarterFrame when message is MidiMessageUnknown(var data, _) => data,
            MidiMessageType.SongPositionPointer when message is MidiMessageSongPositionPointer songPosPointer => new[]
            {
                (byte) songPosPointer.Type,
                songPosPointer.Lsb,
                songPosPointer.Msb
            },
            MidiMessageType.SongSelect when message is MidiMessageSongSelect messageSongSelect => new[]
            {
                (byte) messageSongSelect.Type,
                messageSongSelect.SongNumber
            },
            MidiMessageType.TuneRequest => DefaultMessage(message),
            MidiMessageType.EndOfSysEx => DefaultMessage(message),
            MidiMessageType.TimingClock => DefaultMessage(message),
            MidiMessageType.Start => DefaultMessage(message),
            MidiMessageType.Continue => DefaultMessage(message),
            MidiMessageType.Stop => DefaultMessage(message),
            MidiMessageType.ActiveSensing => DefaultMessage(message),
            MidiMessageType.SystemReset => DefaultMessage(message),

            //TODO Remove undefined?
            MidiMessageType.UndefinedReserved1 => DefaultMessage(message),
            MidiMessageType.UndefinedReserved2 => DefaultMessage(message),
            MidiMessageType.UndefinedReserved3 => DefaultMessage(message),
            MidiMessageType.UndefinedReserved4 => DefaultMessage(message),
            _ => throw new ArgumentOutOfRangeException(nameof(message), "Type of message unknown")
        };

        return messageData;

        byte ByteFrom(MidiMessageType messageType, MidiChannel channel)
        {
            return (byte)((byte)messageType | (byte)channel);
        }

        byte[] DefaultMessage(MidiMessage midiMessage)
        {
            return new[] { (byte)midiMessage.Type };
        }
    }
}