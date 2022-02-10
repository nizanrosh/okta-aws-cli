using Okta.Aws.Cli.Aws.Abstractions;

namespace Okta.Aws.Cli.FileSystem
{
    public interface ICredentialsUpdater
    {
        Task UpdateCredentials(AwsCredentials credentials, CancellationToken cancellationToken);
    }
}
