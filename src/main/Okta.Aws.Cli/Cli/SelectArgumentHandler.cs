using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Aws.ArnMappings;
using Okta.Aws.Cli.Aws.Profiles;
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

    public override Task HandleInternal(CancellationToken cancellationToken)
    {
        var profiles = _profilesService.GetProfiles();
        if (profiles.Count == 0)
        {
            Console.WriteLine("There are no available profiles.");
            return Task.CompletedTask;
        }

        var arnMappings = _arnMappingsService.GetArnMappings();
        var selections = GetSelections(profiles.Keys, arnMappings);

        var selection = Prompt.Select("Select a profile (use arrow keys)", selections);
        var invertedArnMappings = arnMappings.ToDictionary(x => x.Value, x => x.Key);
        if (invertedArnMappings.TryGetValue(selection, out var mapping))
        {
            return UpdateLocalCredentials(profiles[mapping], cancellationToken);
        }

        return UpdateLocalCredentials(profiles[selection], cancellationToken);
    }

    private Task UpdateLocalCredentials(OktaAwsCliProfile profile, CancellationToken cancellationToken)
    {
        var awsCredentials = new AwsCredentials(profile.AccessKeyId, profile.SecretAccessKey, profile.Token,
            profile.Region);
        return _credentialsUpdater.UpdateCredentials(awsCredentials, cancellationToken);
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

public class AesOperation
{
    public static string EncryptString(string key, string plainText)
    {
        byte[] iv = new byte[16];
        byte[] array;

        var rawKey = Encoding.UTF8.GetBytes(key);
        using (Aes aes = Aes.Create())
        {
            aes.Key = rawKey;
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream =
                       new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }

                    array = memoryStream.ToArray();
                }
            }
        }

        return Convert.ToBase64String(array);
    }

    public static string DecryptString(string key, string cipherText)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream =
                       new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}