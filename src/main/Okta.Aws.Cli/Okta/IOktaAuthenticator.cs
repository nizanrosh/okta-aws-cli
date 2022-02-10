using Okta.Auth.Sdk;

namespace Okta.Aws.Cli.Okta
{
    public interface IOktaAuthenticator
    {
        Task<IAuthenticationResponse> Authenticate(CancellationToken cancellationToken);
    }
}
