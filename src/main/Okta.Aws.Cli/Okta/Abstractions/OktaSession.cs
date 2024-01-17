using System.Runtime.Serialization;

namespace Okta.Aws.Cli.Okta.Abstractions
{
    [DataContract]
    public class OktaSession
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        
        [DataMember(Name = "userId")]
        public string UserId { get; set; }
        
        [DataMember(Name = "login")]
        public string Login { get; set; }
        
        [DataMember(Name = "expiresAt")]
        public DateTime ExpiresAt { get; set; }

        public void SanityCheck()
        {
            ArgumentNullException.ThrowIfNull(Id, nameof(Id));
            ArgumentNullException.ThrowIfNull(UserId, nameof(UserId));
            ArgumentNullException.ThrowIfNull(Login, nameof(Login));
        }
    }
}
