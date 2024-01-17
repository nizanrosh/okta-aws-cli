namespace Okta.Aws.Cli.Abstractions;

public class VersionInfo
{
    public string LatestVersion { get; set; }
    public string CurrentVersion { get; set; }
    public DateTime? LastChecked { get; set; }
}

public class VersionInfoWrapper
{
    public VersionInfo VersionInfo { get; }

    public VersionInfoWrapper(VersionInfo versionInfo) => VersionInfo = versionInfo;
}