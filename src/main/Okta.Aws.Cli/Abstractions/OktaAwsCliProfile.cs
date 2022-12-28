namespace Okta.Aws.Cli.Abstractions;

public class OktaAwsCliProfile
{
    public string Key { get; set; }
    public string AccessKeyId { get; set; }
    public string SecretAccessKey { get; set; }
    public string Token { get; set; }
    public string Region { get; set; }
}

public class OktaAwsCliProfileWrapper
{
    public IEnumerable<OktaAwsCliProfile> Profiles { get; }

    public OktaAwsCliProfileWrapper(IEnumerable<OktaAwsCliProfile> profiles) => Profiles = profiles;
}