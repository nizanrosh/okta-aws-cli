using Okta.Aws.Cli.Abstractions;

namespace Okta.Aws.Cli.FileSystem;

public interface IFileVersionUpdater
{
    Task UpdateVersionInfo(VersionInfo versionInfo, CancellationToken cancellationToken);
}