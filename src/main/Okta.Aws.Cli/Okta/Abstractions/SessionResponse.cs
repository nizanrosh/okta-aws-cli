using System.Runtime.Serialization;

namespace Okta.Aws.Cli.Okta.Abstractions
{
    [DataContract]
    public class SessionResponse
    {
        [DataMember(Name = "id")]
        public string? Id { get; set; }
    }
}
