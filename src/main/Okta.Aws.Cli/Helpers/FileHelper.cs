using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Okta.Aws.Cli.Constants;

namespace Okta.Aws.Cli.Helpers;

public static class FileHelper
{
    public static string GetUserAwsFolder(IConfiguration configuration)
    {
        var platform = GetOsPlatform();
        if (platform == OSPlatform.Windows)
            return $"C:/Users/{configuration[LocalSystem.Username]}/.aws";

        if (platform == OSPlatform.Linux)
            return $"/home/{Environment.UserName}/.aws";

        if (platform == OSPlatform.OSX)
            return $"/Users/{Environment.UserName}/.aws";

        throw new ArgumentException($"OS not supported.");
    }

    private static OSPlatform GetOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSPlatform.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSPlatform.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OSPlatform.OSX;
        return OSPlatform.FreeBSD;
    }

    public static string GetUserAwsCredentialsFile(IConfiguration configuration) =>
        $"{GetUserAwsFolder(configuration)}/credentials";

    public static string GetUserSettingsFolder(IConfiguration configuration)
    {
        var platform = GetOsPlatform();
        if (platform == OSPlatform.Windows)
            return $"C:/Users/{configuration[LocalSystem.Username]}/.okta-aws-cli";

        if (platform == OSPlatform.Linux)
            return $"/home/{Environment.UserName}/.okta-aws-cli";

        if (platform == OSPlatform.OSX)
            return $"/Users/{Environment.UserName}/.okta-aws-cli";

        throw new ArgumentException("OS not supported.");
    }

    public static string GetUserSettingsFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/usersettings.json";

    public static string GetUserAwsBackupFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/credentials.backup";

    public static string GetVersionInfoFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/versioninfo.json";

    public static string GetArnMappingsFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/arnmappings.json";
    
    public static string GetProfilesFile(IConfiguration configuration) => 
        $"{GetUserSettingsFolder(configuration)}/profiles.json";
    
    public static string GetSessionFile(IConfiguration configuration) =>
        $"{GetUserSettingsFolder(configuration)}/session.json";
}