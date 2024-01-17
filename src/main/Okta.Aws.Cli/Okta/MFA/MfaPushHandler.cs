using Okta.Auth.Sdk;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Constants;

namespace Okta.Aws.Cli.Okta.MFA
{
    public class MfaPushHandler : MfaHandlerBase
    {
        public override string Type => Factors.Push;

        public override async Task<IAuthenticationResponse> HandleMfa(IAuthenticationClient client, MfaParameters parameters, CancellationToken cancellationToken)
        {
            var verifyResponse = await client.VerifyFactorAsync(GetPushFactorOptions(parameters.FactorId, parameters.StateToken), cancellationToken);

            while (verifyResponse.AuthenticationStatus == AuthenticationStatus.MfaChallenge && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                verifyResponse = await client.VerifyFactorAsync(GetPushFactorOptions(parameters.FactorId, parameters.StateToken), cancellationToken);
            }

            return verifyResponse;
        }

        private VerifyPushFactorOptions GetPushFactorOptions(string factorId, string stateToken) => new()
        {
            FactorId = factorId,
            AutoPush = true,
            StateToken = stateToken,
            RememberDevice = true
        };
    }
}
