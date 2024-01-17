using Okta.Aws.Cli.Cli.Configurations;

namespace Okta.Aws.Cli.Okta.Abstractions.Interfaces;

public interface IOktaApiHttpClient
{
    Task<SamlHtmlResponse> GetSamlHtml(RunConfiguration runConfiguration, string sessionToken, CancellationToken cancellationToken);
}