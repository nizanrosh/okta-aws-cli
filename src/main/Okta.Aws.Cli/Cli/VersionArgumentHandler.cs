using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace Okta.Aws.Cli.Cli;

public class VersionArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "--version";

    public VersionArgumentHandler(IHostApplicationLifetime lifetime) : base(lifetime)
    {
    }

    public override Task HandlerInternal(CancellationToken cancellationToken)
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;

        Console.WriteLine(version);

        return Task.CompletedTask;
    }
}