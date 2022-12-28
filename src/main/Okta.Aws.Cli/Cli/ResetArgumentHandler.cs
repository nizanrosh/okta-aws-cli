using Microsoft.Extensions.Configuration;
using Sharprompt;

namespace Okta.Aws.Cli.Cli;

public class ResetSettingsArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "reset";

    public ResetSettingsArgumentHandler(IConfiguration configuration) : base(configuration)
    {
    }

    protected override async Task HandleInternal(string[] args, CancellationToken cancellationToken)
    {
        var result = Prompt.Confirm("You are about to erase all configurations, are you sure?");
        if (!result) return;
        
        
    }
}