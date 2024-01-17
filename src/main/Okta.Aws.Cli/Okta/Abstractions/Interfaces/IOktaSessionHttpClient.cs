namespace Okta.Aws.Cli.Okta.Abstractions.Interfaces;

public interface IOktaSessionHttpClient
{
    Task<OktaSession> LogIn(string sessionToken, CancellationToken cancellationToken);
}