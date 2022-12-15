using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.FileSystem;
using Okta.Aws.Cli.GitHub;

namespace Okta.Aws.Cli;

public class VersionHostedService : IHostedService
{
    private readonly ILogger<VersionHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IGitHubApiClient _githubClient;
    private readonly IFileVersionUpdater _fileVersionUpdater;

    public VersionHostedService(ILogger<VersionHostedService> logger, IConfiguration configuration, IGitHubApiClient githubClient, IFileVersionUpdater fileVersionUpdater)
    {
        _logger = logger;
        _configuration = configuration;
        _githubClient = githubClient;
        _fileVersionUpdater = fileVersionUpdater;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var task = Task.Run(async () =>
        {
            try
            {
                var versionInfo = _configuration.GetSection(nameof(VersionInfo)).Get<VersionInfo>();

                if (!ShouldQueryForNewVersion(versionInfo)) return;

                var response = await _githubClient.GetRepoTags(cancellationToken);
                if (response.Tags == null || !response.Tags.Any())
                {
                    _logger.LogWarning("Response contained no tags.");
                    return;
                }

                var latestTag = response.Tags.MaxBy(t => t.Name);
                if (latestTag?.Name == null)
                {
                    _logger.LogWarning("Could not find latest tag using MaxBy.");
                    return;
                }

                if (string.Equals(latestTag.Name, versionInfo?.CurrentVersion,
                        StringComparison.InvariantCultureIgnoreCase)) return;

                var newVersionInfo = CreateNewVersionInfo(latestTag.Name);
                await _fileVersionUpdater.UpdateVersionInfoAsync(newVersionInfo, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occurred while handling version info.");
            }
        }, cancellationToken);

        return task;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private bool ShouldQueryForNewVersion(VersionInfo? versionInfo)
    {
        if (versionInfo == null) return true;

        if (!string.Equals($"v{GetAssemblyVersion()}", versionInfo.CurrentVersion))
        {
            var newVersionInfo = CreateNewVersionInfo($"v{GetAssemblyVersion()}");
            _fileVersionUpdater.UpdateVersionInfo(newVersionInfo);
            return false;
        }

        if (versionInfo.LastChecked < DateTime.UtcNow - TimeSpan.FromDays(1)) return true;

        if (!string.Equals(versionInfo.LatestVersion, versionInfo.CurrentVersion,
                StringComparison.InvariantCultureIgnoreCase)) return true;

        return false;
    }

    private VersionInfo CreateNewVersionInfo(string latestTag)
    {
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

        var currentVersion = $"v{assemblyVersion!.Major}.{assemblyVersion!.Minor}.{assemblyVersion!.Build}";

        return new VersionInfo
        {
            CurrentVersion = currentVersion,
            LatestVersion = latestTag,
            LastChecked = DateTime.UtcNow
        };
    }

    private string GetAssemblyVersion()
    {
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

        return $"{assemblyVersion!.Major}.{assemblyVersion!.Minor}.{assemblyVersion!.Build}";
    }
}