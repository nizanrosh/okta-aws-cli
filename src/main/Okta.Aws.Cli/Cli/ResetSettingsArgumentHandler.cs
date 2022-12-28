using Microsoft.Extensions.Configuration;

namespace Okta.Aws.Cli.Cli;

public class ResetSettingsArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "reset";

    public ResetSettingsArgumentHandler(IConfiguration configuration) : base(configuration)
    {
    }

    protected override Task HandleInternal(string[] args, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}