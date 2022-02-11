namespace Okta.Aws.Cli;

public interface IUserSettingsHandler
{
    void SanityCheck();
    Task ConfigureUserSettingsFile(CancellationToken cancellationToken);
}