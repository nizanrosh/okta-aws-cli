using Okta.Auth.Sdk;

namespace Okta.Aws.Cli.Okta.Abstractions.Interfaces
{
    public interface IOktaAuthenticator
    {
        Task<IAuthenticationResponse> Authenticate(CancellationToken cancellationToken);
    }
}
