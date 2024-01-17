namespace Okta.Aws.Cli.Aws.Abstractions
{
    public class AwsCredentials
    {
        public string AccessKeyId { get; }
        public string SecretAccessKey { get; }
        public string SessionToken { get; }
        public string Region { get; }
        public string SelectedAwsAccount { get; set; }
        public string SelectedAwsAccountAlias { get; set; }
        public string FullArn { get; set; }
        public string FullArnAlias { get; set; }

        public AwsCredentials(string accessKeyId, string secretAccessKey, string sessionToken, string region) => (AccessKeyId, SecretAccessKey, SessionToken, Region) = (accessKeyId, secretAccessKey, sessionToken, region);
    }
}
