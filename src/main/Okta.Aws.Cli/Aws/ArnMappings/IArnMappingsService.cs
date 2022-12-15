namespace Okta.Aws.Cli.Aws.ArnMappings;

public interface IArnMappingsService
{
    Task UpdateArnMappingsFile(CancellationToken cancellationToken);
    Dictionary<string, string> GetArnMappings();
}