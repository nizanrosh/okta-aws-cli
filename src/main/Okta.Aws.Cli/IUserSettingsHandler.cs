using Okta.Aws.Cli.Abstractions;

namespace Okta.Aws.Cli;

public interface IUserSettingsHandler
{
    void SanityCheck();
    Task ConfigureUserSettingsFile(CancellationToken cancellationToken);
    Task SaveCurrentUserSettingsToFile(CancellationToken cancellationToken);
    void PrettyPrint(UserSettings userSettings);
}