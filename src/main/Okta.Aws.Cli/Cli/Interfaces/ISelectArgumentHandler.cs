namespace Okta.Aws.Cli.Cli.Interfaces;

public interface ISelectArgumentHandler
{
    Task Handle(string profile, CancellationToken cancellationToken);
}