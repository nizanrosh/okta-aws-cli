using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Okta.Saml;

namespace Okta.Aws.Cli.Aws
{
    public interface IAwsCredentialsProvider
    {
        Task<AwsCredentials> AssumeRole(SamlResult saml, CancellationToken cancellationToken);
    }
}
