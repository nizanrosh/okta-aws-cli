namespace Okta.Aws.Cli.Cli.Interfaces;

public interface ILogoutArgumentHandler
{
    public Task Handle(CancellationToken cancellationToken);
}