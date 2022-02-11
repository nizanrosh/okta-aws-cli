using Okta.Aws.Cli.Aws.Abstractions;

namespace Okta.Aws.Cli.Aws
{
    public interface IAwsCredentialsProvider
    {
        Task<AwsCredentials> AssumeRole(string saml, CancellationToken cancellationToken);
    }
}
