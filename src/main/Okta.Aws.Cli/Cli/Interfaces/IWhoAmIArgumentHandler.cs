namespace Okta.Aws.Cli.Cli.Interfaces;

public interface IWhoAmIArgumentHandler
{
    Task Handle(CancellationToken cancellationToken);
}