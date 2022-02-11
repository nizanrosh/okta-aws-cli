using Okta.Aws.Cli.Okta.Abstractions;
using Okta.Aws.Cli.Okta.Saml;

namespace Okta.Aws.Cli.Okta
{
    public interface IOktaSamlProvider
    {
        Task<SamlResponse> GetSaml(CancellationToken cancellationToken);
    }

    public class OktaSamlProvider : IOktaSamlProvider
    {
        private readonly IOktaAuthenticator _client;
        private readonly ISamlExtractor _extractor;

        public OktaSamlProvider(IOktaAuthenticator client, ISamlExtractor extractor)
        {
            _client = client;
            _extractor = extractor;
        }

        public async Task<SamlResponse> GetSaml(CancellationToken cancellationToken)
        {
            var authenticatedUser = await _client.Authenticate(cancellationToken);
            var saml = await _extractor.ExtractSamlFromHtml(authenticatedUser.SessionToken);

            return saml;
        }
    }
}
