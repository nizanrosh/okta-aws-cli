namespace Okta.Aws.Cli.Aws.Abstractions;

public class ArnsResult
{
    public string FullArn { get; }
    public string PrincipalArn { get; }
    public string RoleArn { get; }
    public string FullArnAlias { get; set; }

    public ArnsResult(string fullArn, string principalArn, string roleArn, string fullArnAlias)
    {
        FullArn = fullArn;
        PrincipalArn = principalArn;
        RoleArn = roleArn;
        FullArnAlias = fullArnAlias;
    }

    public void Deconstruct(out string principalArn, out string roleArn)
    {
        principalArn = PrincipalArn;
        roleArn = RoleArn;
    }
}