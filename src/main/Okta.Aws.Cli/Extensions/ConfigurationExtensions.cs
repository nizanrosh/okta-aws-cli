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

            var versionInfoPath = FileHelper.GetVersionInfoFile(config);
            configBuilder.AddJsonFile(versionInfoPath, true);

            return configBuilder;
        }

        public static IConfigurationBuilder ConfigureArnMappings(this IConfigurationBuilder configBuilder)
        {
            var config = configBuilder.Build();
            var arnMappingsPath = FileHelper.GetArnMappingsFile(config);
            configBuilder.AddJsonFile(arnMappingsPath, true);

            return configBuilder;
        }
    }
}
