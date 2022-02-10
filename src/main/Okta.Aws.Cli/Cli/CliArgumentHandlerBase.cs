using Microsoft.Extensions.Hosting;
using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli.Cli;

public abstract class CliArgumentHandlerBase : ICliArgumentHandler
{
    public abstract string Argument { get; }

    protected readonly IHostApplicationLifetime Lifetime;

    protected CliArgumentHandlerBase(IHostApplicationLifetime lifetime)
    {
        Lifetime = lifetime;
    }

    public virtual async Task Handle(CancellationToken cancellationToken)
    {
        await HandlerInternal(cancellationToken);
        Lifetime.StopApplication();
    }

    public abstract Task HandlerInternal(CancellationToken cancellationToken);
}