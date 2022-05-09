using Okta.Auth.Sdk;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Constants;
using Sharprompt;

namespace Okta.Aws.Cli.Okta.MFA;

public class MfaSmsHandler : MfaHandlerBase
{
    public override string Type => Factors.Sms;

    public override async Task<IAuthenticationResponse> HandleMfa(IAuthenticationClient client, MfaParameters parameters, CancellationToken cancellationToken)
    {
        await client.VerifyFactorAsync(GetSmsFactorOptions(parameters.FactorId, parameters.StateToken), cancellationToken);

        var passCode = Prompt.Input<string>("Enter the SMS pass code");

        var verifyResponse = await client.VerifyFactorAsync(GetSmsFactorOptions(parameters.FactorId, parameters.StateToken, passCode), cancellationToken);

        return verifyResponse;
    }

    private VerifySmsFactorOptions GetSmsFactorOptions(string factorId, string stateToken, string passCode) => new()
    {
        FactorId = factorId,
        StateToken = stateToken,
        PassCode = passCode
    };

    private VerifySmsFactorOptions GetSmsFactorOptions(string factorId, string stateToken) => new()
    {
        FactorId = factorId,
        StateToken = stateToken,
    };
}