namespace Okta.Aws.Cli.Okta.Abstractions
{
    public class SamlResponse
    {
        public string Token { get; }

        public SamlResponse(string token)
        {
            Token = token;
        }
    }
}
