using Okta.Auth.Sdk;

namespace Okta.Aws.Cli.Okta.Abstractions
{
    public interface IOktaAuthenticator
    {
        IAuthenticationClient Client { get; }
        Task<IAuthenticationResponse> Authenticate(CancellationToken cancellationToken);
    }
}
