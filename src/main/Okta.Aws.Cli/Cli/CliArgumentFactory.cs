using Okta.Aws.Cli.Cli.Interfaces;

namespace Okta.Aws.Cli.Cli;

public class CliArgumentFactory : ICliArgumentFactory
{
    private readonly IDictionary<string, ICliArgumentHandler> _handlers;
    private readonly IInvalidArgumentHandler _invalidArgumentHandler;

    public CliArgumentFactory(IEnumerable<ICliArgumentHandler> handlers, IInvalidArgumentHandler invalidArgumentHandler)
    {
        _invalidArgumentHandler = invalidArgumentHandler;
        _handlers = handlers.ToDictionary(h => h.Argument, h => h);
    }

    public ICliArgumentHandler GetHandler(string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            return _invalidArgumentHandler;
        }

        return _handlers.TryGetValue(arg, out var handler) ? handler : _invalidArgumentHandler;
    }
}