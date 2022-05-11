using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Constants;

namespace Okta.Aws.Cli.Helpers;

public static class FileHelper
{
    public static string GetUserAwsFolder(IConfiguration configuration)
    {
        return IsWindows() switch
        {
            true => $"C:/Users/{configuration[LocalSystem.Username]}/.aws",
            false => $"/home/{Environment.UserName}/.aws"
        };
    }

    public static string GetUserAwsCredentialsFile(IConfiguration configuration) =>
        $"{GetUserAwsFolder(configuration)}/credentials";

    public static string GetUserSettingsFolder(IConfiguration configuration)
    {
        return IsWindows() switch
        {
            true => $"C:/Users/{configuration[LocalSystem.Username]}/.okta-aws-cli",
            false => $"/home/{Environment.UserName}/.okta-aws-cli"
        };
    }

    public static string GetUserSettingsFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/usersettings.json";
    public static string GetUserAwsBackupFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/credentials.backup";
    public static string GetVersionInfoFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/versioninfo.json";

    private static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}