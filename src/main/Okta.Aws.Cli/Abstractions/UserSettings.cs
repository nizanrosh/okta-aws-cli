namespace Okta.Aws.Cli.Abstractions
{
    public class UserSettings
    {
        public string? OktaDomain { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? AppUrl { get; set; }
        public string? MfaType { get; set; }
        public string? ProfileName { get; set; }
        //public IEnumerable<string>? ProfileNames { get; set; }
        public string? Region { get; set; }
    }

    public class UserSettingsWrapper
    {
        public UserSettings UserSettings { get; }

        public UserSettingsWrapper(UserSettings userSettings) => UserSettings = userSettings;
    }
}
