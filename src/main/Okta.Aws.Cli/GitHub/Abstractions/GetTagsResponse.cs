using System.Runtime.Serialization;

namespace Okta.Aws.Cli.GitHub.Abstractions;

public class GetTagsResponse
{
    public IList<TagMetadata> Tags { get; set; }
}

[DataContract]
public class TagMetadata
{
    [DataMember(Name = "name")]
    public string Name { get; set; }
}