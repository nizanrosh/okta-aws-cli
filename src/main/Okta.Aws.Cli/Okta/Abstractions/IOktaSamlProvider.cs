using Okta.Aws.Cli.Okta.Saml;

namespace Okta.Aws.Cli.Okta.Abstractions;

public interface IOktaSamlProvider
{
    Task<SamlResult> GetSaml(CancellationToken cancellationToken);
}