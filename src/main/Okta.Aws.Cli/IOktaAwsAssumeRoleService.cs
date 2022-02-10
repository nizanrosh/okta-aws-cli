namespace Okta.Aws.Cli
{
    public interface IOktaAwsAssumeRoleService
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
