using Okta.Aws.Cli.Okta.Abstractions;

namespace Okta.Aws.Cli.Okta.Saml;

public interface ISamlExtractor
{
    SamlResponse ExtractSamlFromHtml(string html);
}