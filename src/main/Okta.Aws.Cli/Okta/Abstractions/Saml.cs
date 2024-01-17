namespace Okta.Aws.Cli.Okta.Abstractions
{
    public class Saml
    {
        public string Token { get; }
        
        public string AppUrl { get; set;  }
        public string Role { get; set; }

        public Saml(string token)
        {
            Token = token;
        }
    }
}
