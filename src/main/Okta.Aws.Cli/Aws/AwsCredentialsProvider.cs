using System.Xml.Serialization;
using Amazon;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Aws.Constants;
using Okta.Aws.Cli.Constants;
using Sharprompt;

namespace Okta.Aws.Cli.Aws
{
    public class AwsCredentialsProvider : IAwsCredentialsProvider
    {
        private readonly ILogger<AwsCredentialsProvider> _logger;
        private readonly IConfiguration _configuration;

        public AwsCredentialsProvider(ILogger<AwsCredentialsProvider> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AwsCredentials> AssumeRole(string saml, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Assuming AWS role...");

            var xmlSerializer = new XmlSerializer(typeof(AssertionModel.Response));
            var assertionXml = xmlSerializer.Deserialize(GetSamlStream(saml)) as AssertionModel.Response;
            ArgumentNullException.ThrowIfNull(assertionXml, nameof(assertionXml));

            var (principalArn, roleArn) = GetArns(assertionXml);

            var assumeRoleRequest = new AssumeRoleWithSAMLRequest
            {
                DurationSeconds = 3600,
                PrincipalArn = principalArn,
                RoleArn = roleArn,
                SAMLAssertion = saml
            };

            var region = _configuration[User.Settings.Region];
            var dummyCredentials = new BasicAWSCredentials("Jack", "Sparrow");
            var stsClient = new AmazonSecurityTokenServiceClient(dummyCredentials, RegionEndpoint.GetBySystemName(region));
            var assumeRoleResponse = await stsClient.AssumeRoleWithSAMLAsync(assumeRoleRequest, cancellationToken);

            return new AwsCredentials(assumeRoleResponse.Credentials.AccessKeyId, assumeRoleResponse.Credentials.SecretAccessKey, assumeRoleResponse.Credentials.SessionToken, region);
        }

        private Stream GetSamlStream(string saml)
        {
            var payload = Convert.FromBase64String(saml);

            var stream = new MemoryStream(payload);
            stream.Position = 0;

            return stream;
        }

        private (string, string) GetArns(AssertionModel.Response assertionResponse)
        {
            var attributeValues = assertionResponse.Assertion.AttributeStatement.Attribute.FirstOrDefault(a => a.Name == Role.Name)?.AttributeValue;
            ArgumentNullException.ThrowIfNull(attributeValues, nameof(attributeValues));

            if(attributeValues.Count > 1)
            {
                Prompt.ColorSchema.Select = ConsoleColor.Yellow;
                var attributeValue = Prompt.Select("Select a role (use arrow keys)", attributeValues);
                return SplitArns(attributeValue);
            }

            return SplitArns(attributeValues.First());
        }

        private (string, string) SplitArns(string attributeValue)
        {
            var arns = attributeValue.Split(',');
            return (arns.First(), arns.Last());
        }
    }
}
