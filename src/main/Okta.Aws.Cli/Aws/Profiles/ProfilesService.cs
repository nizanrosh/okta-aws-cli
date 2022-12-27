using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.FileSystem;

namespace Okta.Aws.Cli.Aws.Profiles;

public class ProfilesService : IProfilesService
{
    private readonly IConfiguration _configuration;
    private readonly IProfilesUpdater _profilesUpdater;
    private readonly Dictionary<string, OktaAwsCliProfile> _profiles;

    public ProfilesService(IConfiguration configuration, IProfilesUpdater profilesUpdater)
    {
        _configuration = configuration;
        _profilesUpdater = profilesUpdater;

        var rawArnMappings = _configuration.GetSection("Profiles").Get<List<OktaAwsCliProfile>>();
        if (rawArnMappings == null)
        {
            _profiles = new Dictionary<string, OktaAwsCliProfile>();
        }
        else
        {
            _profiles = rawArnMappings.ToDictionary(x => x.Key, x => x);
        }
    }

    public Task UpdateProfilesFile(IEnumerable<OktaAwsCliProfile> profiles, CancellationToken cancellationToken)
    {
        return _profilesUpdater.UpdateProfilesAsync(profiles, cancellationToken);
    }

    public Dictionary<string, OktaAwsCliProfile> GetProfiles()
    {
        return _profiles;
    }
}