namespace Okta.Aws.Cli.Abstractions;

public class ArnMappingsWrapper
{
    public List<ArnMapping> ArnMappings { get; }

    public ArnMappingsWrapper(List<ArnMapping> arnMappings) => ArnMappings = arnMappings;
}

public class ArnMapping
{
    public string Key { get; set; }
    public string Value { get; set; }
}