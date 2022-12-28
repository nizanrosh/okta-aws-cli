using System.IO.Compression;
using Amazon.Runtime.Internal.Transform;
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

    public UpdateArgumentHandler(ILogger<UpdateArgumentHandler> logger, IConfiguration configuration, IGitHubApiClient gitHubApiClient) : base(configuration)
    {
        _logger = logger;
        _gitHubApiClient = gitHubApiClient;
    }

    protected override async Task HandleInternal(string[] args, CancellationToken cancellationToken)
    {
        var versionInfo = Configuration.GetSection(nameof(VersionInfo)).Get<VersionInfo>();

        var (result, message) = CanUpdate(versionInfo);
        if (!result)
        {
            _logger.LogError(message);
            //return;
        }

        var (latestVersion, currentVersion) = GetVersions(versionInfo);
        //if(latestVersion <= currentVersion) return;

        var releases = await _gitHubApiClient.GetRepoReleases(cancellationToken);
        if (releases.Releases == null || releases.Releases.Count == 0)
        {
            _logger.LogError("Release download return 0 releases.");
            return;
        }

        var latestRelease = releases.Releases.MaxBy(r => r.Name);
        if (string.IsNullOrEmpty(latestRelease?.ZipballUrl) || string.IsNullOrEmpty(latestRelease.Name))
        {
            //todo - log/exception this
            return;
        }

        Directory.CreateDirectory("release");

        if (!File.Exists($"./release/{latestRelease.Name}.zip"))
        {
            var downloadResult =
                await _gitHubApiClient.DownloadRelease(latestRelease.ZipballUrl, latestRelease.Name, cancellationToken);

            if (!downloadResult)
            {
                //todo - log/exception this
                return;
            }
        }

        ZipFile.ExtractToDirectory($"release/{latestRelease.Name}.zip", $"release/{latestRelease.Name}");
    }

    private (Version, Version) GetVersions(VersionInfo versionInfo)
    {
        var latestVersion = versionInfo!.LatestVersion!.Replace("v","");
        var currentVersion = versionInfo!.CurrentVersion!.Replace("v", "");

        return (Version.Parse(latestVersion), Version.Parse(currentVersion));
    }

    private (bool, string) CanUpdate(VersionInfo? versionInfo)
    {
        if (versionInfo == null || string.IsNullOrEmpty(versionInfo.LatestVersion) ||
            string.IsNullOrEmpty(versionInfo.CurrentVersion))
        {
            var message = "Version information could not be distinguished.";
            return (false, message);
        }

        if (string.Equals(versionInfo.CurrentVersion, versionInfo.LatestVersion))
        {
            var message = $"You are already on the latest version, {versionInfo.CurrentVersion}";
            return (false, message);
        }

        return (true, string.Empty);
    }
}