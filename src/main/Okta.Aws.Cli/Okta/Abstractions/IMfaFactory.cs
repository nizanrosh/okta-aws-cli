namespace Okta.Aws.Cli.Okta.Abstractions
{
    public interface IMfaFactory
    {
        IMfaHandler GetHandler(string type);
    }
}
