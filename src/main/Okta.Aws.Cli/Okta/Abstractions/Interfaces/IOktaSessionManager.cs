namespace Okta.Aws.Cli.Okta.Abstractions.Interfaces;

public interface IOktaSessionManager
{
    Task SaveSession(OktaSession oktaSession, CancellationToken cancellationToken);
    Task<OktaSession> GetSavedSession(CancellationToken cancellationToken);
    void DeleteSession();
}