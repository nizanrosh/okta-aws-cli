using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Helpers;

namespace Okta.Aws.Cli.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder ConfigureUserSettings(this IConfigurationBuilder configBuilder)
        {
            var config = configBuilder.Build();
            var userSettingsPath = FileHelper.GetUserSettingsFile(config);
            configBuilder.AddJsonFile(userSettingsPath, true);

            return configBuilder;
        }
    }
}
