using Okta.Aws.Cli.Abstractions;

namespace Okta.Aws.Cli.Constants
{
    public class User
    {
        public class Settings
        {
            public const string OktaDomain = $"{nameof(UserSettings)}:{nameof(OktaDomain)}";
            public const string Username = $"{nameof(UserSettings)}:{nameof(Username)}";
            public const string Password = $"{nameof(UserSettings)}:{nameof(Password)}";
            public const string AppUrl = $"{nameof(UserSettings)}:{nameof(AppUrl)}";
            public const string MfaType = $"{nameof(UserSettings)}:{nameof(MfaType)}";
            public const string ProfileName = $"{nameof(UserSettings)}:{nameof(ProfileName)}";
            public const string Region = $"{nameof(UserSettings)}:{nameof(Region)}";
            public const string DefaultAwsAccount = $"{nameof(UserSettings)}:{nameof(DefaultAwsAccount)}";
            public const string DefaultAwsAccountAlias = $"{nameof(UserSettings)}:{nameof(DefaultAwsAccountAlias)}";
            public const string DefaultAwsRole = $"{nameof(UserSettings)}:{nameof(DefaultAwsRole)}";
            public const string DefaultAwsRoleAlias = $"{nameof(UserSettings)}:{nameof(DefaultAwsRoleAlias)}";
        }
    }
}
