using System.Runtime.Serialization;

namespace Okta.Aws.Cli.Okta.Abstractions
{
    [DataContract]
    public class AppLink
    {
        [DataMember(Name = "linkUrl")]
        public string LinkUrl { get; set; }

        [DataMember(Name = "appName")]
        public string AppName { get; set; }
        
        [DataMember(Name = "label")]
        public string Label { get; set; }
    }
}
