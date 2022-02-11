using Microsoft.Extensions.Hosting;

namespace Okta.Aws.Cli.Cli;

public class ConfigureArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "configure";

    private readonly IUserSettingsHandler _userSettingsHandler;

    public ConfigureArgumentHandler(IUserSettingsHandler userSettingsHandler, IHostApplicationLifetime lifetime) : base(lifetime)
    {
        _userSettingsHandler = userSettingsHandler;
    }

    public override Task HandlerInternal(CancellationToken cancellationToken)
    {
        return _userSettingsHandler.ConfigureUserSettingsFile(cancellationToken);
    }
}