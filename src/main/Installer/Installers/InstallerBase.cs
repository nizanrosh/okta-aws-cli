using System.Diagnostics;
using Installer.Installers.Interfaces;

namespace Installer.Installers;

public abstract class InstallerBase : IInstaller
{
    public abstract Task Install();
    protected abstract string GetInstallPath();

    protected Process GetInstallProcess(string installPath)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = "../",
            Arguments =
                $"publish src/main/Okta.Aws.Cli/Okta.Aws.Cli.csproj --output {installPath} --source https://api.nuget.org/v3/index.json --configuration Release --verbosity quiet /property:WarningLevel=0"
        });

        return process!;
    }
}