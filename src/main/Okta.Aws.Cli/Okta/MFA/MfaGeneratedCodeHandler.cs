using Okta.Auth.Sdk;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Constants;
using Sharprompt;

namespace Okta.Aws.Cli.Okta.MFA;

public class MfaGeneratedCodeHandler : MfaHandlerBase
{
    public override string Type => Factors.Code;
    public override async Task<IAuthenticationResponse> HandleMfa(IAuthenticationClient client, MfaParameters parameters, CancellationToken cancellationToken)
    {
        var passCode = Prompt.Input<string>("Enter the generated code");

        var verifyResponse = await client.VerifyFactorAsync(GetTotpFactorOptions(parameters.FactorId, parameters.StateToken, passCode), cancellationToken);

        return verifyResponse;
    }

    private VerifyTotpFactorOptions GetTotpFactorOptions(string factorId, string stateToken, string passCode) => new()
    {
        FactorId = factorId,
        StateToken = stateToken,
        PassCode = passCode
    };
}