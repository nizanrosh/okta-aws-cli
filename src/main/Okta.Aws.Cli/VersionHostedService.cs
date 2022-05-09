using System.Reflection;
using System.Runtime.InteropServices;
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

    public async Task StartAsync(CancellationToken cancellationToken)
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

            if (string.Equals(latestTag.Name, versionInfo?.LatestVersion, StringComparison.InvariantCultureIgnoreCase)) return;

            var newVersionInfo = CreateNewVersionInfo(latestTag.Name);
            await _fileVersionUpdater.UpdateVersionInfo(newVersionInfo, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error has occurred while handling version info.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private bool ShouldQueryForNewVersion(VersionInfo? versionInfo)
    {
        if (versionInfo == null) return true;

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
}