using System.Diagnostics.Tracing;
using System.Xml.Serialization;
using Amazon;
using Amazon.IdentityManagement;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Kurukuru;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Aws.ArnMappings;
using Okta.Aws.Cli.Aws.Constants;
using Okta.Aws.Cli.Aws.Profiles;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Saml;
using Sharprompt;

namespace Okta.Aws.Cli.Aws
{
    public class AwsCredentialsProvider : IAwsCredentialsProvider
    {
        private readonly ILogger<AwsCredentialsProvider> _logger;
        private readonly IConfiguration _configuration;
        private readonly IArnMappingsService _arnMappingsService;
        private readonly IProfilesService _profilesService;

        public AwsCredentialsProvider(ILogger<AwsCredentialsProvider> logger, IConfiguration configuration,
            IArnMappingsService arnMappingsService, IProfilesService profilesService)
        {
            _logger = logger;
            _configuration = configuration;
            _arnMappingsService = arnMappingsService;
            _profilesService = profilesService;
        }

        public async Task<AwsCredentials> AssumeRole(SamlResult saml, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Assuming AWS role...");
            var spinner = new Spinner("Assuming Roles...");
            spinner.SymbolSucceed = new SymbolDefinition("V", "V");

            var globalCredentialsMap = new Dictionary<string, AwsCredentials>();

            try
            {
                spinner.Start();

                // var assertionXml = GetAssertionXml(saml.SelectedSaml.Token);
                // ArgumentNullException.ThrowIfNull(assertionXml, nameof(assertionXml));
                //
                // var sessionDuration = GetDuration(assertionXml);
                // var assertionAttributeValues = GetAssertionAttributeValues(assertionXml);
                //
                // var awsCredentialsMap =
                //     await GetSessionAwsCredentials(assertionAttributeValues, saml.SelectedSaml.Token, sessionDuration, cancellationToken);

                var (selectedCredentialsMap, selectedAssertionAttributeValues) =
                    await GetAwsCredentialsMap(saml.SelectedSaml, cancellationToken);

                globalCredentialsMap = globalCredentialsMap.Union(selectedCredentialsMap)
                    .ToDictionary(x => x.Key, x => x.Value);

                foreach (var additionalSaml in saml.AdditionalSamls)
                {
                    var (additionalCredentialsMap, _) =
                        await GetAwsCredentialsMap(additionalSaml, cancellationToken);
                    
                    globalCredentialsMap = globalCredentialsMap.Union(additionalCredentialsMap)
                        .ToDictionary(x => x.Key, x => x.Value);
                }

                await MapArnsToAliases(globalCredentialsMap, cancellationToken);
                await SaveProfiles(globalCredentialsMap, cancellationToken);

                spinner.Succeed();

                var (principalArn, roleArn) = GetArnsToAssume(selectedAssertionAttributeValues);

                var awsCredentials = selectedCredentialsMap[$"{principalArn},{roleArn}"];

                return awsCredentials;
            }
            catch (Exception e)
            {
                spinner.Fail();
                _logger.LogError(e, "An error has occurred while assuming role.");
                throw;
            }
        }

        private async Task<(Dictionary<string, AwsCredentials>, List<string>)> GetAwsCredentialsMap(Saml saml,
            CancellationToken cancellationToken)
        {
            var assertionXml = GetAssertionXml(saml.Token);
            ArgumentNullException.ThrowIfNull(assertionXml, nameof(assertionXml));

            var sessionDuration = GetDuration(assertionXml);
            var assertionAttributeValues = GetAssertionAttributeValues(assertionXml);

            var credentialsMap = await GetSessionAwsCredentials(assertionAttributeValues, saml.Token, sessionDuration,
                cancellationToken);
            return (credentialsMap, assertionAttributeValues);
        }

        private Stream GetSamlStream(string saml)
        {
            var payload = Convert.FromBase64String(saml);

            var stream = new MemoryStream(payload);
            stream.Position = 0;

            return stream;
        }

        private (string, string) GetArnsToAssume(List<string> attributeValues)
        {
            var arnMappings = _arnMappingsService.GetArnMappings();
            var selections = new List<string>();

            foreach (var attributeValue in attributeValues)
            {
                if (arnMappings.TryGetValue(attributeValue, out var selection))
                {
                    selections.Add(selection);
                }
                else
                {
                    selections.Add(attributeValue);
                }
            }

            var invertedArnMappings = arnMappings.ToDictionary(x => x.Value, x => x.Key);

            if (selections.Count > 1)
            {
                Prompt.ColorSchema.Select = ConsoleColor.Yellow;
                var userSelection = Prompt.Select("Select a role (use arrow keys)", selections);
                if (invertedArnMappings.TryGetValue(userSelection, out var attributeValue))
                {
                    return SplitArns(attributeValue);
                }

                return SplitArns(userSelection);
            }

            return SplitArns(attributeValues.First());
        }

        private async Task MapArnsToAliases(Dictionary<string, AwsCredentials> sessionCredentials,
            CancellationToken cancellationToken)
        {
            var shouldUpdateMappingsFile = false;

            var arnMappings = _arnMappingsService.GetArnMappings();

            foreach (var (attributeValue, awsCredentials) in sessionCredentials)
            {
                try
                {
                    if (arnMappings.TryGetValue(attributeValue, out _)) continue;
                    shouldUpdateMappingsFile = true;

                    var (principalArn, roleArn) = SplitArns(attributeValue);
                    var region = _configuration[User.Settings.Region];

                    var awsSessionCredentials = new SessionAWSCredentials(awsCredentials.AccessKeyId,
                        awsCredentials.SecretAccessKey, awsCredentials.SessionToken);

                    var awsIdentityClient =
                        new AmazonIdentityManagementServiceClient(awsSessionCredentials,
                            RegionEndpoint.GetBySystemName(region));

                    var aliases = await awsIdentityClient.ListAccountAliasesAsync(cancellationToken);
                    var customAlias = $"{string.Join('-', aliases.AccountAliases)}-{roleArn.Split('/').Last()}";

                    arnMappings[attributeValue] = customAlias;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"An error has occurred while querying for account alias of {attributeValue}");
                }
            }

            if (shouldUpdateMappingsFile)
            {
                await _arnMappingsService.UpdateArnMappingsFile(cancellationToken);
            }
        }

        private async Task<Dictionary<string, AwsCredentials>> GetSessionAwsCredentials(
            List<string> attributeValues,
            string saml, int duration, CancellationToken cancellationToken)
        {
            var credentials = new Dictionary<string, AwsCredentials>();

            foreach (var attributeValue in attributeValues)
            {
                try
                {
                    var (principalArn, roleArn) = SplitArns(attributeValue);

                    var internalCredentials =
                        await GetInternalAwsCredentials(saml, principalArn, roleArn, duration, cancellationToken);

                    var region = _configuration[User.Settings.Region];
                    var awsCredentials = new AwsCredentials(internalCredentials.AccessKeyId,
                        internalCredentials.SecretAccessKey,
                        internalCredentials.SessionToken, region);

                    credentials[attributeValue] = awsCredentials;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"An error has occurred while getting credentials for {attributeValue}");
                }
            }

            return credentials;
        }

        private (string, string) SplitArns(string attributeValue)
        {
            var arns = attributeValue.Split(',');
            return (arns.First(), arns.Last());
        }

        private int GetDuration(AssertionModel.Response assertionResponse)
        {
            var attributeValues = assertionResponse?.Assertion?.AttributeStatement?.Attribute?
                .FirstOrDefault(a => a.Name == Session.Duration)?.AttributeValue;
            ArgumentNullException.ThrowIfNull(attributeValues, nameof(attributeValues));

            if (attributeValues.Count != 0) return int.Parse(attributeValues.First());

            Prompt.ColorSchema.Select = ConsoleColor.Yellow;
            var attributeValue = Prompt.Input<int>("Please enter session time in seconds");
            return attributeValue;
        }

        private AssertionModel.Response? GetAssertionXml(string saml)
        {
            var xmlSerializer = new XmlSerializer(typeof(AssertionModel.Response));
            var assertionXml = xmlSerializer.Deserialize(GetSamlStream(saml)) as AssertionModel.Response;

            return assertionXml;
        }

        private List<string> GetAssertionAttributeValues(AssertionModel.Response assertionResponse)
        {
            var attributeValues = assertionResponse?.Assertion?.AttributeStatement?.Attribute
                ?.FirstOrDefault(a => a.Name == Role.Name)?.AttributeValue;
            ArgumentNullException.ThrowIfNull(attributeValues, nameof(attributeValues));

            return attributeValues;
        }

        private async Task<AwsCredentials> GetInternalAwsCredentials(string saml, string principalArn, string roleArn,
            int sessionDuration, CancellationToken cancellationToken)
        {
            var assumeRoleRequest = new AssumeRoleWithSAMLRequest
            {
                DurationSeconds = sessionDuration,
                PrincipalArn = principalArn,
                RoleArn = roleArn,
                SAMLAssertion = saml
            };

            var region = _configuration[User.Settings.Region];
            var dummyCredentials = new BasicAWSCredentials("Jack", "Sparrow");
            var stsClient =
                new AmazonSecurityTokenServiceClient(dummyCredentials, RegionEndpoint.GetBySystemName(region));
            var assumeRoleResponse = await stsClient.AssumeRoleWithSAMLAsync(assumeRoleRequest, cancellationToken);

            var credentials = new AwsCredentials(assumeRoleResponse.Credentials.AccessKeyId,
                assumeRoleResponse.Credentials.SecretAccessKey, assumeRoleResponse.Credentials.SessionToken, region);

            return credentials;
        }

        private async Task SaveProfiles(Dictionary<string, AwsCredentials> credentials,
            CancellationToken cancellationToken)
        {
            var profiles = credentials.Select(x =>
            {
                var (key, value) = x;
                return new OktaAwsCliProfile
                {
                    Key = key,
                    Region = value.Region,
                    AccessKeyId = value.AccessKeyId,
                    SecretAccessKey = value.SecretAccessKey,
                    Token = value.SessionToken
                };
            });

            await _profilesService.UpdateProfilesFile(profiles, cancellationToken);
        }
    }
}