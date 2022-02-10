using Microsoft.Extensions.Hosting;

namespace Okta.Aws.Cli.Cli;

public class RunArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "run";

    private readonly IOktaAwsAssumeRoleService _assumeRoleService;
    private readonly IUserSettingsHandler _userSettingsHandler;

    public RunArgumentHandler(IOktaAwsAssumeRoleService assumeRoleService, IUserSettingsHandler userSettingsHandler, IHostApplicationLifetime lifetime) : base(lifetime)
    {
        _assumeRoleService = assumeRoleService;
        _userSettingsHandler = userSettingsHandler;
    }

    public override async Task HandlerInternal(CancellationToken cancellationToken)
    {
        Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Okta-Aws-Cli"));

        _userSettingsHandler.SanityCheck();
        await _assumeRoleService.RunAsync(cancellationToken);

        Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Goodbye"));
    }
}