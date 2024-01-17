using Okta.Aws.Cli.Cli.Configurations;
using Okta.Aws.Cli.Okta.Saml;

namespace Okta.Aws.Cli.Okta.Abstractions.Interfaces;

public interface IOktaSamlProvider
{
    Task<SamlResult> GetSaml(RunConfiguration runConfiguration, CancellationToken cancellationToken);
}