using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Okta.Aws.Cli.Cli;

public class VersionArgumentHandler : CliArgumentHandlerBase
{
    public override string Argument => "--version";

    public VersionArgumentHandler(IHostApplicationLifetime lifetime, IConfiguration configuration) : base(lifetime, configuration)
    {
    }

    public override Task HandlerInternal(CancellationToken cancellationToken)
    {
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

        var version = $"{assemblyVersion!.Major}.{assemblyVersion!.Minor}.{assemblyVersion!.Build}";

        Console.WriteLine(version);

        return Task.CompletedTask;
    }
}