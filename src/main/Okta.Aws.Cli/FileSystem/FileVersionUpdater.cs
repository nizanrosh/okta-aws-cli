using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Helpers;

namespace Okta.Aws.Cli.FileSystem;

public class FileVersionUpdater : IFileVersionUpdater
{
    private readonly IConfiguration _configuration;

    public FileVersionUpdater(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task UpdateVersionInfoAsync(VersionInfo versionInfo, CancellationToken cancellationToken)
    {
        var versionCacheFolder = FileHelper.GetUserSettingsFolder(_configuration);

        if (!Directory.Exists(versionCacheFolder))
        {
            Directory.CreateDirectory(versionCacheFolder);
            return UpdateVersionInfoInternalAsync(versionInfo, cancellationToken);
        }

        return UpdateVersionInfoInternalAsync(versionInfo, cancellationToken);
    }

    public void UpdateVersionInfo(VersionInfo versionInfo)
    {
        var versionCacheFolder = FileHelper.GetUserSettingsFolder(_configuration);

        if (!Directory.Exists(versionCacheFolder))
        {
            Directory.CreateDirectory(versionCacheFolder);
            UpdateVersionInfoInternal(versionInfo);
            return;
        }

        UpdateVersionInfoInternal(versionInfo);
    }

    private Task UpdateVersionInfoInternalAsync(VersionInfo versionInfo, CancellationToken cancellationToken)
    {
        var wrapper = new VersionInfoWrapper(versionInfo);

        var payload = JsonConvert.SerializeObject(wrapper);
        return File.WriteAllTextAsync(FileHelper.GetVersionInfoFile(_configuration), payload, cancellationToken);
    }

    private void UpdateVersionInfoInternal(VersionInfo versionInfo)
    {
        var wrapper = new VersionInfoWrapper(versionInfo);

        var payload = JsonConvert.SerializeObject(wrapper);
        File.WriteAllText(FileHelper.GetVersionInfoFile(_configuration), payload);
    }
}