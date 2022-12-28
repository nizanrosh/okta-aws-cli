namespace Okta.Aws.Cli.Cli.Interfaces;

public interface ICliArgumentHandler
{
    string Argument { get; }
    Task Handle(string[] args, CancellationToken cancellationToken);
}