using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Cli.Configurations;

namespace Okta.Aws.Cli.Cli.Interfaces;

public interface IRunArgumentHandler
{
    public Task<AwsCredentials> Handle(RunConfiguration runConfiguration, CancellationToken cancellationToken);
}