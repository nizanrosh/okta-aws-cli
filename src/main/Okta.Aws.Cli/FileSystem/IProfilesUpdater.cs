using Okta.Aws.Cli.Abstractions;

namespace Okta.Aws.Cli.FileSystem;

public interface IProfilesUpdater
{
    Task UpdateProfilesAsync(IEnumerable<OktaAwsCliProfile> profiles, CancellationToken cancellationToken);
}