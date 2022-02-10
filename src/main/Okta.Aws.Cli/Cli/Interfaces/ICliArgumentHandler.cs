namespace Okta.Aws.Cli.Cli.Interfaces;

public interface ICliArgumentHandler
{
    string Argument { get; }
    Task Handle(CancellationToken cancellationToken);
}