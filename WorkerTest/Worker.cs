using RtMidi.Net;

namespace WorkerTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private MidiInputClient? _midiInputClient;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var devices = MidiManager.GetAvailableDevices();
            foreach (var d in devices)
            {
                _logger.LogInformation($"{d.Port}) {d.Name} - {d.Type}");
            }
            if (devices.Any())
            {
                //TODO Change device to test
                var devicePort = 0u;
                var device = MidiManager.GetDeviceInfo(devicePort, RtMidi.Net.Enums.MidiDeviceType.Input);
                _midiInputClient = new MidiInputClient(device);
                _midiInputClient.OnMessageReceived += MidiClient_OnMessageReceived;
                _midiInputClient.ActivateMessageReceivedEvent();
                _midiInputClient.Open();
            }
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _midiInputClient?.Close();
            _midiInputClient?.Dispose();
            return base.StopAsync(cancellationToken);
        }


        private void MidiClient_OnMessageReceived(object? sender, RtMidi.Net.Events.MidiMessageReceivedEventArgs args)
        {
            if(args.Message is MidiMessageNote message)
            {
                _logger.LogInformation($"{message.Note.GetName()} - {message.Type}");
            }
            else
            {
                _logger.LogInformation($"{args.Message.Type}");
            }
        }
    }
}