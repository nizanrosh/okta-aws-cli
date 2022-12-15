using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Aws;
using Okta.Aws.Cli.FileSystem;
using Okta.Aws.Cli.Okta.Abstractions;

namespace Okta.Aws.Cli
{
    public class OktaAwsAssumeRoleService : IOktaAwsAssumeRoleService
    {
        private readonly ILogger<OktaAwsAssumeRoleService> _logger;
        private readonly IOktaSamlProvider _oktaSamlProvider;
        private readonly IAwsCredentialsProvider _awsAuthenticator;
        private readonly ICredentialsUpdater _credentialsUpdater;

        public OktaAwsAssumeRoleService(ILogger<OktaAwsAssumeRoleService> logger, IOktaSamlProvider oktaSamlProvider, IAwsCredentialsProvider awsAuthenticator, ICredentialsUpdater credentialsUpdater)
        {
            _logger = logger;
            _oktaSamlProvider = oktaSamlProvider;
            _awsAuthenticator = awsAuthenticator;
            _credentialsUpdater = credentialsUpdater;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _oktaSamlProvider.GetSaml(cancellationToken);
                var credentials = await _awsAuthenticator.AssumeRole(response.Token, cancellationToken);
                await _credentialsUpdater.UpdateCredentials(credentials, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occurred.");
            }
        }
    }
}
