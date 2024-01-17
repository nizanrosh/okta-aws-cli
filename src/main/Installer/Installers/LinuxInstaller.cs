using Abstractions.Consts;

namespace Installer.Installers;

public class LinuxInstaller : InstallerBase
{
    public override async Task Install()
    {
        var appPath = GetInstallPath();

        var process = GetInstallProcess(appPath);
        await process.WaitForExitAsync();
        
        Console.WriteLine("Adding app to machine path...\n");
        
        var linuxProfileFile = $"/home/{Environment.UserName}/.profile";
        var paths = await GetLinuxPaths(linuxProfileFile);

        if (InstallerHelper.ShouldUpdateLinuxPaths(paths, appPath))
        {
            var newPaths = InstallerHelper.GetNewLinuxPaths(paths, appPath);
            await File.WriteAllLinesAsync(linuxProfileFile, newPaths);
        }

        await InstallerHelper.MakeOacliExecutable(appPath);
    }

    protected override string GetInstallPath()
    {
        var appPath = Constants.LinuxAppPath;
        Directory.CreateDirectory(appPath);

        return appPath;
    }

    private async ValueTask<string[]> GetLinuxPaths(string linuxProfileFile)
    {
        if(File.Exists(linuxProfileFile)) return await File.ReadAllLinesAsync(linuxProfileFile);

        return Array.Empty<string>();
    }
}