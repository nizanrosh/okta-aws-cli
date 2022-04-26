using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Okta.Auth.Sdk;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Sdk.Abstractions.Configuration;

namespace Okta.Aws.Cli.Okta
{

    public class OktaAuthenticator : IOktaAuthenticator
    {
        private readonly ILogger<OktaAuthenticator> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMfaFactory _mfaFactory;

        public OktaAuthenticator(ILogger<OktaAuthenticator> logger, IConfiguration configuration, IMfaFactory mfaFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _mfaFactory = mfaFactory;
        }

        public async Task<IAuthenticationResponse> Authenticate(CancellationToken cancellationToken)
        {
            var client = GetOktaClient();

            var settings = _configuration.GetSection(nameof(UserSettings)).Get<UserSettings>();

            _logger.LogInformation($"Starting Okta user authentication, user: {settings.Username}.");

            var authenticationResponse = await client.AuthenticateAsync(new AuthenticateOptions
            {
                Username = settings.Username,
                Password = settings.Password
            }, cancellationToken);


            if (authenticationResponse.AuthenticationStatus == AuthenticationStatus.MfaRequired)
            {
                _logger.LogInformation($"MFA required, prompting for MFA of type {settings.MfaType}, check your phone.");

                var factorId = GetFactorId(authenticationResponse, settings.MfaType!);

                authenticationResponse = await _mfaFactory.GetHandler(settings.MfaType!).HandleMfa(client, new MfaParameters(factorId, authenticationResponse.StateToken), cancellationToken);

                return authenticationResponse;
            }

            _logger.LogInformation("Okta Authentication success.");

            return authenticationResponse;
        }

        private string GetFactorId(IAuthenticationResponse authenticationResponse, string mfaType)
        {
            var resources = JsonConvert.DeserializeObject<AuthenticationFactors>(authenticationResponse.Embedded.GetRaw());
            var factorId = resources?.Factors?.FirstOrDefault(f => f.FactorType == mfaType)!.Id;
            ArgumentNullException.ThrowIfNull(factorId, nameof(factorId));

            return factorId;
        }

        private IAuthenticationClient GetOktaClient()
        {
            return new AuthenticationClient(new OktaClientConfiguration
            {
                OktaDomain = _configuration[User.Settings.OktaDomain]
            });
        }
    }
}
