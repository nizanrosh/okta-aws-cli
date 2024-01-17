using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli.Cli;

public class ConfigureArgumentHandler : IConfigureArgumentHandler
{

    private readonly IUserSettingsHandler _userSettingsHandler;
    private readonly ILogoutArgumentHandler _logoutArgumentHandler;

    public ConfigureArgumentHandler(IUserSettingsHandler userSettingsHandler, ILogoutArgumentHandler logoutArgumentHandler)
    {
        _userSettingsHandler = userSettingsHandler;
        _logoutArgumentHandler = logoutArgumentHandler;
    }

    public async Task Handle(CancellationToken cancellationToken)
    {
        var logoutTask = _logoutArgumentHandler.Handle(cancellationToken);
        var configureTask = _userSettingsHandler.ConfigureUserSettingsFile(cancellationToken);

        await Task.WhenAll(logoutTask, configureTask);
    }
}