using Figgle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Okta.Aws.Cli.Cli;

public class RunArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "run";

    private readonly IOktaAwsAssumeRoleService _assumeRoleService;
    private readonly IUserSettingsHandler _userSettingsHandler;

    public RunArgumentHandler(IOktaAwsAssumeRoleService assumeRoleService, IUserSettingsHandler userSettingsHandler, IConfiguration configuration) : base(configuration)
    {
        _assumeRoleService = assumeRoleService;
        _userSettingsHandler = userSettingsHandler;
    }

    protected override async Task HandleInternal(string[] args, CancellationToken cancellationToken)
    {
        Console.WriteLine(FiggleFonts.Standard.Render("Okta-Aws-Cli"));

        _userSettingsHandler.SanityCheck();
        await _assumeRoleService.RunAsync(cancellationToken);

        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine(FiggleFonts.Standard.Render("Goodbye"));
    }
}