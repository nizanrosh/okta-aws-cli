using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Saml;

namespace Okta.Aws.Cli.Okta
{
    public class OktaSamlProvider : IOktaSamlProvider
    {
        private readonly IOktaAuthenticator _client;
        private readonly ISamlExtractor _extractor;
        private readonly IOktaApiClient _oktaApiClient;

        public OktaSamlProvider(IOktaAuthenticator client, ISamlExtractor extractor, IOktaApiClient oktaApiClient)
        {
            _client = client;
            _extractor = extractor;
            _oktaApiClient = oktaApiClient;
        }

        public async Task<SamlResponse> GetSaml(CancellationToken cancellationToken)
        {
            var authenticatedUser = await _client.Authenticate(cancellationToken);
            var html = await _oktaApiClient.GetSamlHtml(authenticatedUser.SessionToken, cancellationToken);
            var saml = _extractor.ExtractSamlFromHtml(html);

            return saml;
        }
    }
}
