using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Okta.Aws.Cli.Cli;

public class ConfigureArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "configure";

    private readonly IUserSettingsHandler _userSettingsHandler;

    public ConfigureArgumentHandler(IUserSettingsHandler userSettingsHandler, IHostApplicationLifetime lifetime, IConfiguration configuration) : base(lifetime, configuration)
    {
        _userSettingsHandler = userSettingsHandler;
    }

    public override Task HandleInternal(CancellationToken cancellationToken)
    {
        return _userSettingsHandler.ConfigureUserSettingsFile(cancellationToken);
    }
}