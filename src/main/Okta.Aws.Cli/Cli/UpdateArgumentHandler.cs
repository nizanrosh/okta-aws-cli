using System.Net.Cache;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.GitHub;

namespace Okta.Aws.Cli.Cli;

public class UpdateArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "update";

    private readonly ILogger<UpdateArgumentHandler> _logger;
    private readonly IGitHubApiClient _gitHubApiClient;

    public UpdateArgumentHandler(ILogger<UpdateArgumentHandler> logger, IHostApplicationLifetime lifetime, IConfiguration configuration, IGitHubApiClient gitHubApiClient) : base(lifetime, configuration)
    {
        _logger = logger;
        _gitHubApiClient = gitHubApiClient;
    }

    public override async Task HandlerInternal(CancellationToken cancellationToken)
    {
        var versionInfo = Configuration.GetSection(nameof(VersionInfo)).Get<VersionInfo>();
        if (!CanUpdate(versionInfo)) return;

        var (latestVersion, currentVersion) = GetVersions(versionInfo);
        //if(latestVersion <= currentVersion) return;

        var releases = await _gitHubApiClient.GetRepoReleases(cancellationToken);
        if (releases.Releases == null || releases.Releases.Count == 0)
        {
            _logger.LogError("Release download return 0 releases.");
            return;
        }

        var latestRelease = releases.Releases.MaxBy(r => r.Name);

        Directory.CreateDirectory("release");

        var result =
            await _gitHubApiClient.DownloadRelease(latestRelease.ZipballUrl, latestRelease.Name, cancellationToken);
    }

    private (Version, Version) GetVersions(VersionInfo versionInfo)
    {
        var latestVersion = versionInfo!.LatestVersion!.Replace("v","");
        var currentVersion = versionInfo!.CurrentVersion!.Replace("v", "");

        return (Version.Parse(latestVersion), Version.Parse(currentVersion));
    }

    private bool CanUpdate(VersionInfo? versionInfo)
    {
        if (versionInfo == null || string.IsNullOrEmpty(versionInfo.LatestVersion) || string.IsNullOrEmpty(versionInfo.CurrentVersion)) return false;

        return true;
    }
}