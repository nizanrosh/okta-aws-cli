using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Helpers;
using Sharprompt;

namespace Okta.Aws.Cli.Cli;

public class ResetArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "reset";

    public ResetArgumentHandler(IConfiguration configuration) : base(configuration)
    {
    }

    protected override Task HandleInternal(string[] args, CancellationToken cancellationToken)
    {
        Prompt.ColorSchema.Hint = ConsoleColor.Red;
        Prompt.ColorSchema.PromptSymbol = ConsoleColor.Red;
        var result = Prompt.Confirm("You are about to erase all configurations, are you sure?");
        if (!result) return Task.CompletedTask;

        var directory = FileHelper.GetUserSettingsFolder(Configuration);
        Directory.Delete(directory, true);

        return Task.CompletedTask;
    }
}