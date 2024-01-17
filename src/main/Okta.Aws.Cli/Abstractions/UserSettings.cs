namespace Okta.Aws.Cli.Abstractions
{
    public class UserSettings
    {
        public string OktaDomain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AppUrl { get; set; }
        public string MfaType { get; set; }
        public string ProfileName { get; set; }
        public string Region { get; set; }
        public string DefaultAwsAccount { get; set; }
        public string DefaultAwsAccountAlias { get; set; }
        public string DefaultAwsRole { get; set; }
        public string DefaultAwsRoleAlias { get; set; }
    }

    public class UserSettingsWrapper
    {
        public UserSettings UserSettings { get; }

        public UserSettingsWrapper(UserSettings userSettings) => UserSettings = userSettings;
    }
}
