using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli;

public class MainAppHostedService : IHostedService
{
    private readonly ICliArgumentFactory _argumentFactory;

    public MainAppHostedService(ICliArgumentFactory argumentFactory)
    {
        _argumentFactory = argumentFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var args = Environment.GetCommandLineArgs();

        var handler = _argumentFactory.GetHandler(args.ElementAtOrDefault(1));

        var task = Task.Run(async () => await handler.Handle(cancellationToken), cancellationToken);

        return task;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}