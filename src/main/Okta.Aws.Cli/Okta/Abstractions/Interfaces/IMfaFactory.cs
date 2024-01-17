namespace Okta.Aws.Cli.Okta.Abstractions.Interfaces
{
    public interface IMfaFactory
    {
        IMfaHandler GetHandler(string type);
    }
}
