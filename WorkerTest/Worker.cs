using RtMidi.Net;
using RtMidi.Net.Clients;
using RtMidi.Net.Enums;
using RtMidi.Net.Events;

namespace WorkerTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private MidiInputClient? _midiInputClient;
        private const bool UseEventHandler = true; //Change to test

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting");
            var rtMidiVersion = MidiManager.GetRtMidiVersion();
            _logger.LogInformation("RtMidi version: {rtMidiVersion}", rtMidiVersion);
            var devices = MidiManager.GetAvailableDevices();
            if (devices.Any())
            {
                _logger.LogInformation("Available devices:");
                foreach (var d in devices.OrderBy(info => info.Port))
                {
                    _logger.LogInformation("{port}) {name} - {type}", d.Port, d.Name, d.Type);
                }

                var devicePort = 1u; //Change device to test
                var device = MidiManager.GetDeviceInfo(devicePort, MidiDeviceType.Input);
                _midiInputClient = new MidiInputClient(device);
                if (UseEventHandler)
                {
                    _midiInputClient.OnMessageReceived += MidiClient_OnMessageReceived;
                    _midiInputClient.ActivateMessageReceivedEvent();
                }
                _midiInputClient.Open();
            }
            else
            {
                var exception = new Exception("No Midi devices found");
                _logger.LogError(exception, "No Midi devices found");
                throw exception;
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing");
            if (!UseEventHandler)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var (message, _) = await _midiInputClient!.GetMessageAsync(stoppingToken);
                    OnMessageReceived(message);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping");
            _midiInputClient?.Close();
            _midiInputClient?.Dispose();
            await base.StopAsync(cancellationToken);
        }

        private void MidiClient_OnMessageReceived(object? sender, MidiMessageReceivedEventArgs args)
        {
            OnMessageReceived(args.Message);
        }

        private void OnMessageReceived(MidiMessage midiMessage)
        {
            if (midiMessage is MidiMessageNote { Type: MidiMessageType.NoteOn or MidiMessageType.NoteOff } message)
            {
                const int offset = 21;
                var note = message.Note.GetByteRepresentation() - offset;
                _logger.LogInformation("{noteName} - {messageType}, Note number: {noteNumber}", message.Note.GetName(), message.Type, note);
            }
            else
            {
                _logger.LogInformation("{messageType}", midiMessage.Type);
            }
        }
    }
}