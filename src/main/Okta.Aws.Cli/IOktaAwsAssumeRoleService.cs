using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Cli.Configurations;

namespace Okta.Aws.Cli
{
    public interface IOktaAwsAssumeRoleService
    {
        Task<AwsCredentials> RunAsync(RunConfiguration runConfiguration, CancellationToken cancellationToken);
    }
}
