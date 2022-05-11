using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli.Cli;

public abstract class CliArgumentHandlerBase : ICliArgumentHandler
{
    public abstract string Argument { get; }

    protected readonly IHostApplicationLifetime Lifetime;
    protected readonly IConfiguration Configuration;

    protected CliArgumentHandlerBase(IHostApplicationLifetime lifetime, IConfiguration configuration)
    {
        Lifetime = lifetime;
        Configuration = configuration;
    }

    public virtual async Task Handle(CancellationToken cancellationToken)
    {
        await HandleInternal(cancellationToken);
        CheckForUpdates();
        Lifetime.StopApplication();
    }

    public abstract Task HandleInternal(CancellationToken cancellationToken);

    private void CheckForUpdates()
    {
        var versionInfo = Configuration.GetSection(nameof(VersionInfo)).Get<VersionInfo>();
        if (versionInfo == null || string.Equals(versionInfo.CurrentVersion, versionInfo.LatestVersion)) return;

        PrintNewVersionAvailable(versionInfo);
    }

    private void PrintNewVersionAvailable(VersionInfo versionInfo)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("warning: ");
        Console.ResetColor();
        Console.Write($"A new version of Okta-Aws-Cli is available. To upgrade from version '{versionInfo.CurrentVersion}' to '{versionInfo.LatestVersion}', visit https://github.com/nizanrosh/okta-aws-cli");
    }
}