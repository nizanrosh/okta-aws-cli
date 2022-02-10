namespace Okta.Aws.Cli.Aws.Abstractions
{
    public class AwsCredentials
    {
        public string AccessKeyId { get; }
        public string SecretAccessKey { get; }
        public string SessionToken { get; }
        public string Region { get; }

        public AwsCredentials(string accessKeyId, string secretAccessKey, string sessionToken, string region) => (AccessKeyId, SecretAccessKey, SessionToken, Region) = (accessKeyId, secretAccessKey, sessionToken, region);
    }
}
