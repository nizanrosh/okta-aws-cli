namespace Okta.Aws.Cli.Cli.Interfaces;

public interface IConfigureArgumentHandler
{
    public Task Handle(CancellationToken cancellationToken);
}