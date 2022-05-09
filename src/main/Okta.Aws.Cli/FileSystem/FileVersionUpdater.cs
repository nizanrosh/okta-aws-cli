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

    public Task UpdateVersionInfo(VersionInfo versionInfo, CancellationToken cancellationToken)
    {
        var versionCacheFolder = FileHelper.GetUserSettingsFolder(_configuration);

        if (!Directory.Exists(versionCacheFolder))
        {
            Directory.CreateDirectory(versionCacheFolder);
            return UpdateVersionInfoInternal(versionInfo, cancellationToken);
        }

        return UpdateVersionInfoInternal(versionInfo, cancellationToken);
    }

    private Task UpdateVersionInfoInternal(VersionInfo versionInfo, CancellationToken cancellationToken)
    {
        var wrapper = new VersionInfoWrapper(versionInfo);

        var payload = JsonConvert.SerializeObject(wrapper);
        return File.WriteAllTextAsync(FileHelper.GetVersionInfoFile(_configuration), payload, cancellationToken);
    }
}