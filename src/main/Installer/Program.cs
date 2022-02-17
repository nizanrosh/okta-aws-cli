using System.Diagnostics;
using System.Runtime.InteropServices;
using Installer;

Console.WriteLine("Installing okta-aws-cli...");

var installerPath = Directory.GetCurrentDirectory();
var rootFolder = Directory.GetParent(installerPath)?.FullName;
ArgumentNullException.ThrowIfNull(rootFolder, nameof(rootFolder));

var appPath = Path.Combine(rootFolder, "app");

var process = Process.Start(new ProcessStartInfo
{
    FileName = "dotnet",
    WorkingDirectory = "../",
    Arguments = "publish --output app --source https://api.nuget.org/v3/index.json"
});

await process!.WaitForExitAsync();

Console.WriteLine("Adding app to machine path...");

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var name = "PATH";
    var scope = EnvironmentVariableTarget.Machine;
    var oldValue = Environment.GetEnvironmentVariable(name, scope);

    if (InstallerHelper.ShouldUpdateWindowsPaths(oldValue!, appPath))
    {
        var newPaths = InstallerHelper.GetNewWindowsPaths(oldValue!, appPath);
        Environment.SetEnvironmentVariable(name, newPaths, scope);
    }
}
else
{
    var linuxProfileFile = $"/home/{Environment.UserName}/.profile";
    var paths = await File.ReadAllLinesAsync(linuxProfileFile);

    if (InstallerHelper.ShouldUpdateLinuxPaths(paths, appPath))
    {
        var newPaths = InstallerHelper.GetNewLinuxPaths(paths, appPath);
        await File.WriteAllLinesAsync(linuxProfileFile, newPaths);
    }
}



Console.WriteLine("Done.");