namespace Okta.Aws.Cli.Okta.Abstractions;

public interface IOktaApiClient
{
    Task<string> GetSamlHtml(string sessionToken, CancellationToken cancellationToken);
}