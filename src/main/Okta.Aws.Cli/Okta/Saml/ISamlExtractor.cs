using Okta.Aws.Cli.Okta.Abstractions;

namespace Okta.Aws.Cli.Okta.Saml;

public interface ISamlExtractor
{
    SamlResult ExtractSamlFromHtml(SamlHtmlResponse html);
}