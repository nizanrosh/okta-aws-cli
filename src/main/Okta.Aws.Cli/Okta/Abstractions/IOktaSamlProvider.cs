namespace Okta.Aws.Cli.Okta.Abstractions;

public interface IOktaSamlProvider
{
    Task<SamlResponse> GetSaml(CancellationToken cancellationToken);
}