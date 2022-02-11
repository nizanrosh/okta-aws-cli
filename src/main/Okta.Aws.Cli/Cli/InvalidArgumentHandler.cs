using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli.Cli;

public class InvalidArgumentHandler : CliArgumentHandlerBase, IInvalidArgumentHandler
{
    public override string Argument => "invalid";

    private readonly IEnumerable<ICliArgumentHandler> _handlers;

    public InvalidArgumentHandler(IEnumerable<ICliArgumentHandler> handlers, IHostApplicationLifetime lifetime) : base(lifetime)
    {
        _handlers = handlers;
    }

    public override Task HandlerInternal(CancellationToken cancellationToken)
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