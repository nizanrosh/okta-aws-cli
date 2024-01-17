using System.Diagnostics;
using Abstractions.Consts;
using Installer.Installers.Interfaces;

namespace Installer.Installers;

public class WindowsInstaller : InstallerBase
{
    public override async Task Install()
    {
        var appPath = GetInstallPath();

        var process = GetInstallProcess(appPath);
        await process.WaitForExitAsync();

        Console.WriteLine("Adding app to machine path...\n");
        
        var name = "PATH";
        var scope = EnvironmentVariableTarget.Machine;
        var oldValue = Environment.GetEnvironmentVariable(name, scope);

        var newPaths = InstallerHelper.GetNewWindowsPaths(oldValue!, appPath);
        Environment.SetEnvironmentVariable(name, newPaths, scope);
    }

    protected override string GetInstallPath()
    {
        var appPath = Constants.WindowsAppPath;
        Directory.CreateDirectory(appPath);

        return appPath;
    }
}