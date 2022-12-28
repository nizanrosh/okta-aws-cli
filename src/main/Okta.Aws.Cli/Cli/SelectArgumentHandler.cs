using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Aws.ArnMappings;
using Okta.Aws.Cli.Aws.Profiles;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.FileSystem;
using Sharprompt;

namespace Okta.Aws.Cli.Cli;

public class SelectArgumentHandler : CliArgumentHandlerBase
{
    private readonly IProfilesService _profilesService;
    private readonly ICredentialsUpdater _credentialsUpdater;
    private readonly IArnMappingsService _arnMappingsService;
    public override string Argument => "select";

    public SelectArgumentHandler(IConfiguration configuration, IProfilesService profilesService,
        ICredentialsUpdater credentialsUpdater, IArnMappingsService arnMappingsService) : base(configuration)
    {
        _profilesService = profilesService;
        _credentialsUpdater = credentialsUpdater;
        _arnMappingsService = arnMappingsService;
    }

    protected override Task HandleInternal(string[] args, CancellationToken cancellationToken)
    {
        var profiles = _profilesService.GetProfiles();
        if (profiles.Count == 0)
        {
            Console.WriteLine("There are no available profiles.");
            return Task.CompletedTask;
        }

        var profileName = GetProfileName(args);

        var arnMappings = _arnMappingsService.GetArnMappings();
        var selections = GetSelections(profiles.Keys, arnMappings);

        var selection = Prompt.Select("Select a profile (use arrow keys)", selections);
        var invertedArnMappings = arnMappings.ToDictionary(x => x.Value, x => x.Key);
        if (invertedArnMappings.TryGetValue(selection, out var mapping))
        {
            return UpdateLocalCredentials(profileName, profiles[mapping], cancellationToken);
        }

        return UpdateLocalCredentials(profileName, profiles[selection], cancellationToken);
    }

    private string GetProfileName(string[] args)
    {
        var potentialArg = args.ElementAtOrDefault(1);
        if (string.IsNullOrEmpty(potentialArg)) return GetProfileNameFromConfig();

        if (potentialArg == "--profile")
        {
            var potentialProfileName = args.ElementAtOrDefault(2);
            if (string.IsNullOrEmpty(potentialProfileName))
            {
                Console.WriteLine("Invalid profile name provided.");
                throw new ArgumentException("Invalid profile name provided.");
            }

            return potentialProfileName;
        }

        return GetProfileNameFromConfig();
    }

    private string GetProfileNameFromConfig()
    {
        var profileName = Configuration[User.Settings.ProfileName];
        if (string.IsNullOrEmpty(profileName)) return "default";

        return profileName;
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