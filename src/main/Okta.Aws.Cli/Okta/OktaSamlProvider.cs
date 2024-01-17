using Kurukuru;
using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Cli.Configurations;
using Okta.Aws.Cli.Constants;
using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Abstractions.Interfaces;
using Okta.Aws.Cli.Okta.Saml;

namespace Okta.Aws.Cli.Okta
{
    public class OktaSamlProvider : IOktaSamlProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IOktaAuthenticator _client;
        private readonly ISamlExtractor _extractor;
        private readonly IOktaApiHttpClient _oktaApiHttpClient;
        private readonly IOktaSessionHttpClient _oktaSessionHttpClient;
        private readonly IOktaSessionManager _oktaSessionManager;

        public OktaSamlProvider(IConfiguration configuration, IOktaAuthenticator client, ISamlExtractor extractor,
            IOktaApiHttpClient oktaApiHttpClient, IOktaSessionHttpClient oktaSessionHttpClient,
            IOktaSessionManager oktaSessionManager)
        {
            _configuration = configuration;
            _client = client;
            _extractor = extractor;
            _oktaApiHttpClient = oktaApiHttpClient;
            _oktaSessionHttpClient = oktaSessionHttpClient;
            _oktaSessionManager = oktaSessionManager;
        }

        public async Task<SamlResult> GetSaml(RunConfiguration runConfiguration, CancellationToken cancellationToken)
        {
            var sessionId = await GetSessionId(cancellationToken);

            var samlHtmlResponse = await _oktaApiHttpClient.GetSamlHtml(runConfiguration, sessionId, cancellationToken);
            var saml = _extractor.ExtractSamlFromHtml(samlHtmlResponse);
            saml.SelectedAppUrl = samlHtmlResponse.SelectedAppUrl;

            return saml;
        }

        private async Task<string> GetSessionId(CancellationToken cancellationToken)
        {
            var localSession = await _oktaSessionManager.GetSavedSession(cancellationToken);
            if (localSession?.Id != null && localSession.ExpiresAt > DateTime.UtcNow - TimeSpan.FromMinutes(1))
            {
                await Spinner.StartAsync($"Logged in as {localSession.Login}.", () => Task.CompletedTask);
                return localSession.Id;
            }

            var authenticatedUser = await _client.Authenticate(cancellationToken);
            var oktaSession = await _oktaSessionHttpClient.LogIn(authenticatedUser.SessionToken, cancellationToken);
            await _oktaSessionManager.SaveSession(oktaSession, cancellationToken);

            return oktaSession.Id;
        }
    }
}