namespace Okta.Aws.Cli.Cli.Interfaces;

public interface IResetArgumentHandler
{
    Task Handle(CancellationToken cancellationToken);
}