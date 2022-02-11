using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli;

public class HostedService : IHostedService
{
    private readonly ICliArgumentFactory _argumentFactory;

    public HostedService(ICliArgumentFactory argumentFactory)
    {
        _argumentFactory = argumentFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var args = Environment.GetCommandLineArgs();

        await _argumentFactory.GetHandler(args.ElementAtOrDefault(1)).Handle(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}