using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Okta.Aws.Cli.Aws;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.FileSystem;
using Okta.Aws.Cli.Okta.Abstractions;

namespace Okta.Aws.Cli
{
    public class OktaAwsAssumeRoleService : IOktaAwsAssumeRoleService
    {
        private readonly ILogger<OktaAwsAssumeRoleService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOktaSamlProvider _oktaSamlProvider;
        private readonly IAwsCredentialsProvider _awsAuthenticator;
        private readonly ICredentialsUpdater _credentialsUpdater;

        public OktaAwsAssumeRoleService(ILogger<OktaAwsAssumeRoleService> logger, IConfiguration configuration, IOktaSamlProvider oktaSamlProvider, IAwsCredentialsProvider awsAuthenticator, ICredentialsUpdater credentialsUpdater)
        {
            _logger = logger;
            _configuration = configuration;
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
                await _credentialsUpdater.UpdateCredentials(_configuration[User.Settings.ProfileName], credentials, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error has occurred.");
            }
        }
    }
}
