using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Cli.Interfaces;
using Okta.Aws.Cli.Okta.Abstractions.Interfaces;

namespace Okta.Aws.Cli.Cli;

public class WhoAmIArgumentHandler : IWhoAmIArgumentHandler
{
    private readonly IConfiguration _configuration;
    private readonly IUserSettingsHandler _userSettingsHandler;
    private readonly IOktaSessionManager _oktaSessionManager;

    public WhoAmIArgumentHandler(IConfiguration configuration, IUserSettingsHandler userSettingsHandler,
        IOktaSessionManager oktaSessionManager)
    {
        _configuration = configuration;
        _userSettingsHandler = userSettingsHandler;
        _oktaSessionManager = oktaSessionManager;
    }

    public async Task Handle(CancellationToken cancellationToken)
    {
        var userSettings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

        if (userSettings == null)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("warning: ");
            Console.ResetColor();
            Console.Write("I couldn't find any user settings, run 'oacli configure' first.\n");
        }
        else
        {
            _userSettingsHandler.PrettyPrint(userSettings);
        }

        var session = await _oktaSessionManager.GetSavedSession(cancellationToken);
        if (session == null) return;
        session.SanityCheck();
        
        Console.WriteLine();
        Console.Write("Logged in as ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"{session.Login}.\n");
        Console.ResetColor();
        Console.WriteLine($"Session will expire at {session.ExpiresAt} UTC.");
    }
}