using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Cli.Interfaces;
using Okta.Aws.Cli.Helpers;
using Sharprompt;

namespace Okta.Aws.Cli.Cli;

public class ResetArgumentHandler : IResetArgumentHandler
{
    private readonly IConfiguration _configuration;

    public ResetArgumentHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task Handle( CancellationToken cancellationToken)
    {
        Prompt.ColorSchema.Hint = ConsoleColor.Red;
        Prompt.ColorSchema.PromptSymbol = ConsoleColor.Red;
        var result = Prompt.Confirm("You are about to erase all configurations, are you sure?");
        if (!result) return Task.CompletedTask;

        var directory = FileHelper.GetUserSettingsFolder(_configuration);
        Directory.Delete(directory, true);

        return Task.CompletedTask;
    }
}