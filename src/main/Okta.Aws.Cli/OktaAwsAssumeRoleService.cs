using Okta.Aws.Cli.Aws;
using Okta.Aws.Cli.FileSystem;
using Okta.Aws.Cli.Okta;
using Okta.Aws.Cli.Okta.Abstractions;

namespace Okta.Aws.Cli
{
    public class OktaAwsAssumeRoleService : IOktaAwsAssumeRoleService
    {
        private readonly IOktaSamlProvider _oktaSamlProvider;
        private readonly IAwsCredentialsProvider _awsAuthenticator;
        private readonly ICredentialsUpdater _credentialsUpdater;

        public OktaAwsAssumeRoleService(IOktaSamlProvider oktaSamlProvider, IAwsCredentialsProvider awsAuthenticator, ICredentialsUpdater credentialsUpdater)
        {
            _oktaSamlProvider = oktaSamlProvider;
            _awsAuthenticator = awsAuthenticator;
            _credentialsUpdater = credentialsUpdater;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var response = await _oktaSamlProvider.GetSaml(cancellationToken);
            var credentials = await _awsAuthenticator.AssumeRole(response.Token, cancellationToken);
            await _credentialsUpdater.UpdateCredentials(credentials, cancellationToken);
        }
    }
}
