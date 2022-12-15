using System.Xml.Serialization;
using Amazon;
using Amazon.IdentityManagement;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Aws.ArnMappings;
using Okta.Aws.Cli.Aws.Constants;
using Okta.Aws.Cli.Constants;
using Sharprompt;

namespace Okta.Aws.Cli.Aws
{
    public class AwsCredentialsProvider : IAwsCredentialsProvider
    {
        private readonly ILogger<AwsCredentialsProvider> _logger;
        private readonly IConfiguration _configuration;
        private readonly IArnMappingsService _arnMappingsService;

        public AwsCredentialsProvider(ILogger<AwsCredentialsProvider> logger, IConfiguration configuration, IArnMappingsService arnMappingsService)
        {
            _logger = logger;
            _configuration = configuration;
            _arnMappingsService = arnMappingsService;
        }

        public async Task<AwsCredentials> AssumeRole(string saml, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Assuming AWS role...");

            var assertionXml = GetAssertionXml(saml);
            ArgumentNullException.ThrowIfNull(assertionXml, nameof(assertionXml));

            var sessionDuration = GetDuration(assertionXml);
            var assertionAttributeValues = GetAssertionAttributeValues(assertionXml);

            await MapArnsToAliases(assertionAttributeValues, saml, sessionDuration, cancellationToken);

            var (principalArn, roleArn) = GetArnsToAssume(assertionAttributeValues);

            var credentials =
                await GetInternalAwsCredentials(saml, principalArn, roleArn, sessionDuration, cancellationToken);

            return credentials;
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

        private async Task MapArnsToAliases(List<string> attributeValues, string saml, int duration, CancellationToken cancellationToken)
        {
            var shouldUpdateMappingsFile = false;

            var arnMappings = _arnMappingsService.GetArnMappings();

            foreach (var attributeValue in attributeValues)
            {
                try
                {
                    if (!arnMappings.TryGetValue(attributeValue, out _))
                    {
                        shouldUpdateMappingsFile = true;

                        var (principalArn, roleArn) = SplitArns(attributeValue);

                        var internalCredentials =
                            await GetInternalAwsCredentials(saml, principalArn, roleArn, duration, cancellationToken);

                        var awsCredentials = new SessionAWSCredentials(internalCredentials.AccessKeyId, internalCredentials.SecretAccessKey,
                            internalCredentials.SessionToken);

                        var awsIdentityClient = new AmazonIdentityManagementServiceClient(awsCredentials, RegionEndpoint.EUWest1);

                        var aliases = await awsIdentityClient.ListAccountAliasesAsync(cancellationToken);
                        var customAlias = $"{string.Join('-', aliases.AccountAliases)}-{roleArn.Split('/').Last()}";

                        arnMappings[attributeValue] = customAlias;
                    }
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
            var attributeValues = assertionResponse?.Assertion?.AttributeStatement?.Attribute?.FirstOrDefault(a => a.Name == Role.Name)?.AttributeValue;
            ArgumentNullException.ThrowIfNull(attributeValues, nameof(attributeValues));

            return attributeValues;
        }

        private async Task<AwsCredentials> GetInternalAwsCredentials(string saml, string principalArn, string roleArn, int sessionDuration, CancellationToken cancellationToken)
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
            var stsClient = new AmazonSecurityTokenServiceClient(dummyCredentials, RegionEndpoint.GetBySystemName(region));
            var assumeRoleResponse = await stsClient.AssumeRoleWithSAMLAsync(assumeRoleRequest, cancellationToken);

            var credentials = new AwsCredentials(assumeRoleResponse.Credentials.AccessKeyId, assumeRoleResponse.Credentials.SecretAccessKey, assumeRoleResponse.Credentials.SessionToken, region);

            return credentials;
        }
    }
}
