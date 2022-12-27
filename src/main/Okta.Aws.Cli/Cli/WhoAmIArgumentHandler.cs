using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Abstractions;

namespace Okta.Aws.Cli.Cli;

public class WhoAmIArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "whoami";

    private readonly IUserSettingsHandler _userSettingsHandler;

    public WhoAmIArgumentHandler(IConfiguration configuration, IUserSettingsHandler userSettingsHandler) : base(configuration)
    {
        _userSettingsHandler = userSettingsHandler;
    }

    public override Task HandleInternal(CancellationToken cancellationToken)
    {
        var userSettings = Configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

        if (userSettings == null)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("warning: ");
            Console.ResetColor();
            Console.Write("I could not find any user settings, who are you?\n");
        }
        else
        {
            _userSettingsHandler.PrettyPrint(userSettings);
        }

        return Task.CompletedTask;
    }
}