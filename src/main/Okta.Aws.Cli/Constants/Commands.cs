namespace Okta.Aws.Cli.Constants;

public class Commands
{
    public const string Run = "run";
    public const string Configure = "configure";
    public const string Logout = "logout";
    public const string Reset = "reset";
    public const string Select = "select";
    public const string WhoAmI = "whoami";

    public class Sub
    {
        public const string Save = "save";
        public const char ShortSave = 's';
        
        public const string SessionTime = "session-time";
        public const char ShortSessionTime = 't';

        public const string Fresh = "fresh";
        public const char ShortFresh = 'f';

        public const string Profile = "profile";
        public const char ShortProfile = 'p';
    }
}