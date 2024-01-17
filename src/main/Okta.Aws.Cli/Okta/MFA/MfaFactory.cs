using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Abstractions.Interfaces;

namespace Okta.Aws.Cli.Okta.MFA
{

    public class MfaFactory : IMfaFactory
    {
        public Dictionary<string, IMfaHandler> _handlers { get; }

        public MfaFactory(IEnumerable<IMfaHandler> handlers)
        {
            _handlers = handlers.ToDictionary(h => h.Type, h => h);
        }

        public IMfaHandler GetHandler(string type)
        {
            if(_handlers.TryGetValue(type, out var handler))
            {
                return handler;
            }

            throw new KeyNotFoundException($"There is no MFA handler {type}.");
        }
    }
}
