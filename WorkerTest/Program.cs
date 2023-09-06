using WorkerTest;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .ConfigureHostOptions(options => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore)
    .Build();

await host.RunAsync();
