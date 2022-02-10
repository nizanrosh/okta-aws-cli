using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Constants;

namespace Okta.Aws.Cli.Helpers;

public static class FileHelper
{
    public static string GetUserAwsFolder(IConfiguration configuration)
    {
        if(IsWindows()) return $"C:/Users/{configuration[LocalSystem.Username]}/.aws";

        return $"/home/{Environment.UserName}/.aws";
    }

    public static string GetUserAwsCredentialsFile(IConfiguration configuration) =>
        $"{GetUserAwsFolder(configuration)}/credentials";

    public static string GetUserSettingsFolder(IConfiguration configuration)
    {
        if(IsWindows()) return $"C:/Users/{configuration[LocalSystem.Username]}/.okta-aws-cli";

        return $"/home/{Environment.UserName}/.okta-aws-cli";
    }

    public static string GetUserSettingsFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/usersettings.json";
    public static string GetUserAwsBackupFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/credentials.backup";

    private static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}