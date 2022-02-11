namespace Okta.Aws.Cli.Okta.Abstractions
{
    public class SessionRequest
    {
        public string SessionToken { get; set; }

        public SessionRequest(string sessionToken)
        {
            SessionToken = sessionToken;
        }
    }
}
