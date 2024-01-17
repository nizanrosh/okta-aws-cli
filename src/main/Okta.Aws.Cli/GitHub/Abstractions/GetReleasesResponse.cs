using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Okta.Aws.Cli.GitHub.Abstractions;

public class GetReleasesResponse
{
    public IList<ReleaseMetadata> Releases { get; set; }
}

[DataContract]
public class ReleaseMetadata
{
    [JsonPropertyName("name")]
    [DataMember(Name = "name")]
    public string Name { get; set; }

    [JsonPropertyName("tarball_url")]
    [DataMember(Name = "tarball_url")]
    public string TarballUrl { get; set; }

    [JsonPropertyName("zipball_url")]
    [DataMember(Name = "zipball_url")]
    public string ZipballUrl { get; set; }
}