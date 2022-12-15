using Okta.Aws.Cli.Abstractions;

namespace Okta.Aws.Cli.FileSystem;

public interface IFileVersionUpdater
{
    Task UpdateVersionInfoAsync(VersionInfo versionInfo, CancellationToken cancellationToken);
    void UpdateVersionInfo(VersionInfo versionInfo);
}