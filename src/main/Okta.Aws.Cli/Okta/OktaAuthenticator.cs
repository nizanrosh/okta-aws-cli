using Kurukuru;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Okta.Auth.Sdk;
using Okta.Aws.Cli.Abstractions;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Encryption;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Sdk.Abstractions;
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

            var authenticationResponse = await GetAuthenticationResponse(client, settings, cancellationToken);

            if (authenticationResponse.AuthenticationStatus == AuthenticationStatus.MfaRequired)
            {
                authenticationResponse =
                    await GetMfaAuthenticationResponse(client, settings, authenticationResponse, cancellationToken);

                return authenticationResponse;
            }

            _logger.LogInformation("Okta Authentication success.");

            return authenticationResponse;
        }

        private async Task<IAuthenticationResponse> GetAuthenticationResponse(IAuthenticationClient client, UserSettings settings, CancellationToken cancellationToken)
        {
            using var spinner = new Spinner($"Starting authentication for {settings.Username}");
            spinner.SymbolSucceed = new SymbolDefinition("V", "V");

            try
            {
                spinner.Start();

                _logger.LogInformation($"Starting Okta user authentication, user: {settings.Username}.");

                var authenticationResponse = await client.AuthenticateAsync(new AuthenticateOptions
                {
                    Username = settings.Username,
                    Password = AesOperation.DecryptString(settings.Password!)
                }, cancellationToken);

                spinner.Succeed();

                return authenticationResponse;
            }
            catch (OktaApiException oktaApiException)
            {
                _logger.LogError(oktaApiException, "An okta api exception has occurred.");
                spinner.Fail("Authentication failed, are you sure your password is correct?");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                spinner.Fail();
                throw;
            }
        }

        private async Task<IAuthenticationResponse> GetMfaAuthenticationResponse(IAuthenticationClient client,
            UserSettings settings, IAuthenticationResponse originalResponse, CancellationToken cancellationToken)
        {
            using var spinner = new Spinner($"Prompting for MFA of type {settings.MfaType}, check your phone.");
            spinner.SymbolSucceed = new SymbolDefinition("V", "V");

            try
            {
                spinner.Start();

                _logger.LogInformation($"MFA required, prompting for MFA of type {settings.MfaType}, check your phone.");

                var factorId = GetFactorId(originalResponse, settings.MfaType!);

                var authenticationResponse = await _mfaFactory.GetHandler(settings.MfaType!).HandleMfa(client, new MfaParameters(factorId, originalResponse.StateToken), cancellationToken);

                spinner.Succeed();

                return authenticationResponse;
            }
            catch (Exception)
            {
                spinner.Fail();
                throw;
            }
        }

        private string GetFactorId(IAuthenticationResponse authenticationResponse, string mfaType)
        {
            var resources = JsonConvert.DeserializeObject<AuthenticationFactors>(authenticationResponse.Embedded.GetRaw());
            var factorId = resources.Factors?.FirstOrDefault(f => f.FactorType == mfaType)?.Id;
            if (string.IsNullOrEmpty(factorId))
            {
                throw new ArgumentNullException(nameof(factorId),
                    $"Could not get factor id for MFA {mfaType}.");
            }

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
