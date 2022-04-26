using System.Runtime.Serialization;

namespace Okta.Aws.Cli.Okta.Abstractions
{
    [DataContract]
    public class Factor
    {

        [DataMember(Name = "id")]
        public string? Id { get; set; }

        [DataMember(Name = "factorType")]
        public string? FactorType { get; set; }

        [DataMember(Name = "provider")]
        public string? Provider { get; set; }

        [DataMember(Name = "vendorName")]
        public string? VendorName { get; set; }
    }

    [DataContract]
    public class AuthenticationFactors
    {

        [DataMember(Name = "factors")]
        public IList<Factor>? Factors { get; set; }
    }
}
