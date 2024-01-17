using Figgle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Aws.Abstractions;
using Okta.Aws.Cli.Cli.Configurations;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli.Cli;

public class RunArgumentHandler : IRunArgumentHandler
{
    private readonly IOktaAwsAssumeRoleService _assumeRoleService;
    private readonly IUserSettingsHandler _userSettingsHandler;

    public RunArgumentHandler(IOktaAwsAssumeRoleService assumeRoleService, IUserSettingsHandler userSettingsHandler)
    {
        _assumeRoleService = assumeRoleService;
        _userSettingsHandler = userSettingsHandler;
    }

    public async Task<AwsCredentials> Handle(RunConfiguration runConfiguration, CancellationToken cancellationToken)
    {
        Console.WriteLine(FiggleFonts.Standard.Render("Okta-Aws-Cli"));

        _userSettingsHandler.SanityCheck();
        var credentials = await _assumeRoleService.RunAsync(runConfiguration, cancellationToken);

        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine(FiggleFonts.Standard.Render("Goodbye"));

        return credentials;
    }
}