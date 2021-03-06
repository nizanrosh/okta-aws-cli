using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli.Cli;

public class InvalidArgumentHandler : CliArgumentHandlerBase, IInvalidArgumentHandler
{
    public override string Argument => "invalid";

    private readonly IEnumerable<ICliArgumentHandler> _handlers;

    public InvalidArgumentHandler(IEnumerable<ICliArgumentHandler> handlers, IHostApplicationLifetime lifetime, IConfiguration configuration) : base(lifetime, configuration)
    {
        _handlers = handlers;
    }

    public override Task HandleInternal(CancellationToken cancellationToken)
    {
        Console.WriteLine("okta-aws-cli: argument command: invalid choice, valid choices are:\n");

        foreach (var cliArgumentHandler in _handlers)
        {
            Console.WriteLine(cliArgumentHandler.Argument);
        }

        Console.WriteLine();

        return Task.CompletedTask;
    }
}