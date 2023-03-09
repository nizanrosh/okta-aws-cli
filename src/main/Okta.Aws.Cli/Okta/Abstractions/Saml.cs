namespace Okta.Aws.Cli.Okta.Abstractions
{
    public class Saml
    {
        public string Token { get; }

        public Saml(string token)
        {
            Token = token;
        }
    }
}
