using Okta.Aws.Cli.Abstractions;

namespace Okta.Aws.Cli.Aws.Profiles;

public interface IProfilesService
{
    Dictionary<string, OktaAwsCliProfile> GetProfiles();
    Task UpdateProfilesFile(IEnumerable<OktaAwsCliProfile> profiles, CancellationToken cancellationToken);
}