using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Aws.ArnMappings;
using Okta.Aws.Cli.Aws.Profiles;
using Okta.Aws.Cli.Cli.Interfaces;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.FileSystem;
using Sharprompt;

namespace Okta.Aws.Cli.Cli;

public class SelectArgumentHandler : ISelectArgumentHandler
{
    private readonly IProfilesService _profilesService;
    private readonly ICredentialsUpdater _credentialsUpdater;
    private readonly IArnMappingsService _arnMappingsService;
    
    public SelectArgumentHandler(IProfilesService profilesService,
        ICredentialsUpdater credentialsUpdater, IArnMappingsService arnMappingsService)
    {
        _profilesService = profilesService;
        _credentialsUpdater = credentialsUpdater;
        _arnMappingsService = arnMappingsService;
    }

    public Task Handle(string profileName, CancellationToken cancellationToken)
    {
        var profiles = _profilesService.GetProfiles();
        if (profiles.Count == 0)
        {
            Console.WriteLine("There are no available profiles.");
            return Task.CompletedTask;
        }

        var arnMappings = _arnMappingsService.GetArnMappings();
        var selections = GetSelections(profiles.Keys, arnMappings);

        Prompt.ColorSchema.Select = ConsoleColor.Yellow;
        var selection = Prompt.Select("Select a profile (use arrow keys)", selections);
        var invertedArnMappings = arnMappings.ToDictionary(x => x.Value, x => x.Key);
        if (invertedArnMappings.TryGetValue(selection, out var mapping))
        {
            return UpdateLocalCredentials(profileName, profiles[mapping], cancellationToken);
        }

        return UpdateLocalCredentials(profileName, profiles[selection], cancellationToken);
    }

    private Task UpdateLocalCredentials(string profileName, OktaAwsCliProfile profile, CancellationToken cancellationToken)
    {
        var awsCredentials = new AwsCredentials(profile.AccessKeyId, profile.SecretAccessKey, profile.Token,
            profile.Region);
        return _credentialsUpdater.UpdateCredentials(profileName, awsCredentials, cancellationToken);
    }

    private List<string> GetSelections(IEnumerable<string> profiles, Dictionary<string, string> arnsMappings)
    {
        var result = new List<string>();

        foreach (var profile in profiles)
        {
            if (arnsMappings.TryGetValue(profile, out var mapping))
            {
                result.Add(mapping);
                continue;
            }

            result.Add(profile);
        }

        return result;
    }
}