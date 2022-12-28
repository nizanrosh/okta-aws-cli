using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli.Cli;

public abstract class CliArgumentHandlerBase : ICliArgumentHandler
{
    public abstract string Argument { get; }

    protected readonly IConfiguration Configuration;

    protected CliArgumentHandlerBase(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public virtual async Task Handle(string[] args, CancellationToken cancellationToken)
    {
        await HandleInternal(args, cancellationToken);
        CheckForUpdates();
    }

    protected abstract Task HandleInternal(string[] args, CancellationToken cancellationToken);

    private void CheckForUpdates()
    {
        var versionInfo = Configuration.GetSection(nameof(VersionInfo)).Get<VersionInfo>();
        if (versionInfo == null || string.Equals($"v{GetAppVersion()}", versionInfo.LatestVersion)) return;

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

    protected string GetAppVersion()
    {
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

        return $"{assemblyVersion!.Major}.{assemblyVersion!.Minor}.{assemblyVersion!.Build}";
    }
}