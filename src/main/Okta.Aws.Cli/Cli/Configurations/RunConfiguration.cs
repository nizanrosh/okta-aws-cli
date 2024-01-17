namespace Okta.Aws.Cli.Cli.Configurations;

public class RunConfiguration
{
    public int SessionTime { get; }
    public string AppUrl { get; }
    public string AppUrlAlias { get; }
    public string FullArn { get; }
    public string FullArnAlias { get; }
    public string SubCommand { get; }
    
    public RunConfiguration()
    {
        
    }
    
    public RunConfiguration(int sessionTime, string subCommand)
    {
        SessionTime = sessionTime;
        SubCommand = subCommand;
    }

    public RunConfiguration(int sessionTime, string appUrl, string appUrlAlias, string fullArn, string fullArnAlias, string subCommand)
    {
        SessionTime = sessionTime;
        AppUrl = appUrl;
        AppUrlAlias = appUrlAlias;
        FullArn = fullArn;
        FullArnAlias = fullArnAlias;
        SubCommand = subCommand;
    }
}