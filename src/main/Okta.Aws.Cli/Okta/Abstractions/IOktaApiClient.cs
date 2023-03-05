namespace Okta.Aws.Cli.Okta.Abstractions;

public interface IOktaApiClient
{
    Task<SamlHtmlResponse> GetSamlHtml(string sessionToken, CancellationToken cancellationToken);
}