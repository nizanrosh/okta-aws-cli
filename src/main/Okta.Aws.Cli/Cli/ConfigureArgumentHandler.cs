using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Okta.Aws.Cli.Cli;

public class ConfigureArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "configure";

    private readonly IUserSettingsHandler _userSettingsHandler;

    public ConfigureArgumentHandler(IUserSettingsHandler userSettingsHandler, IConfiguration configuration) : base(configuration)
    {
        _userSettingsHandler = userSettingsHandler;
    }

    protected override Task HandleInternal(string[] args, CancellationToken cancellationToken)
    {
        return _userSettingsHandler.ConfigureUserSettingsFile(cancellationToken);
    }
}