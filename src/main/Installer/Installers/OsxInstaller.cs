using Abstractions.Consts;

namespace Installer.Installers;

public class OsxInstaller : InstallerBase
{
    public override async Task Install()
    {
        var appPath = GetInstallPath();

        var process = GetInstallProcess(appPath);
        await process.WaitForExitAsync();
        
        Console.WriteLine("Adding app to machine path...\n");
        
        var pathsFile = "/etc/paths.d/okta-aws-cli";

        await File.WriteAllTextAsync(pathsFile, appPath);

        await InstallerHelper.MakeOacliExecutable(appPath);
    }

    protected override string GetInstallPath()
    {
        var appPath = Constants.OsxAppPath;
        Directory.CreateDirectory(appPath);

        return appPath;
    }
}