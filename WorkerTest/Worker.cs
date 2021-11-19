using RtMidi.Net;

namespace WorkerTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private MidiInputClient _midiInputClient;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var devices = MidiManager.GetAvailableDevices();
            var device = MidiManager.GetDeviceInfo(0, RtMidi.Net.Enums.MidiDeviceType.Input);
            _midiInputClient = new MidiInputClient(device);
            _midiInputClient.OnMessageReceived += MidiClient_OnMessageReceived;
            _midiInputClient.ActivateMessageReceivedEvent();
            _midiInputClient.Open();
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(5000, stoppingToken);
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    stoppingToken
            //}
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _midiInputClient.Close();
            _midiInputClient.Dispose();
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