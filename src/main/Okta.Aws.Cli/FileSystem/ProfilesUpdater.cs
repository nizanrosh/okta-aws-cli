using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Helpers;

namespace Okta.Aws.Cli.FileSystem;

public class ProfilesUpdater : IProfilesUpdater
{
    private readonly IConfiguration _configuration;

    public ProfilesUpdater(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public Task UpdateProfilesAsync(IEnumerable<OktaAwsCliProfile> profiles, CancellationToken cancellationToken)
    {
        var cacheFolder = FileHelper.GetUserSettingsFolder(_configuration);

        if (!Directory.Exists(cacheFolder))
        {
            Directory.CreateDirectory(cacheFolder);
            return UpdateVersionInfoInternalAsync(profiles, cancellationToken);
        }

        return UpdateVersionInfoInternalAsync(profiles, cancellationToken);
    }
    
    private Task UpdateVersionInfoInternalAsync(IEnumerable<OktaAwsCliProfile> profiles, CancellationToken cancellationToken)
    {
        var wrapper = new OktaAwsCliProfileWrapper(profiles);
        
        var payload = JsonConvert.SerializeObject(wrapper);
        return File.WriteAllTextAsync(FileHelper.GetProfilesFile(_configuration), payload, cancellationToken);
    }
    
    
}