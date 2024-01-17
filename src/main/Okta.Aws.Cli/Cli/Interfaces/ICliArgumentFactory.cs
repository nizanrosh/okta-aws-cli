namespace Okta.Aws.Cli.Cli.Interfaces;

public interface ICliArgumentFactory
{
    ICliArgumentHandler GetHandler(string arg);
}