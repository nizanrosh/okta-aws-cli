using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Installer;

public static class InstallerHelper
{
    public static string GetNewWindowsPaths(string oldPath, string appPath)
    {
        string newValue;

        if (oldPath.EndsWith(';'))
        {
            newValue = oldPath + $"{appPath};";
        }
        else
        {
            newValue = oldPath + $";{appPath};";
        }

        return newValue;
    }

    public static bool ShouldUpdateWindowsPaths(string oldPath, string appPath) =>
        !oldPath.Contains(appPath, StringComparison.InvariantCultureIgnoreCase);

    public static IEnumerable<string> GetNewLinuxPaths(string[] oldPaths, string appPath)
    {
        var linuxAppPath = $"export PATH=\"$PATH:{appPath}\"";
        return oldPaths.Concat(new[] { linuxAppPath });
    }

    public static bool ShouldUpdateLinuxPaths(string[] paths, string appPath) => !paths.Any(p => p.Equals($"export PATH=\"$PATH:{appPath}\""));

    public static Task MakeOacliExecutable(string path)
    {
        var oacliExecutable = Process.Start(new ProcessStartInfo
        {
            FileName = "chmod",
            WorkingDirectory = path,
            Arguments =
                "+x oacli"
        });

        return oacliExecutable!.WaitForExitAsync();
    }

    public static string GetAppPath(IConfiguration configuration)
    {
        var path = configuration["path"];
        if (!string.IsNullOrEmpty(path)) return path;

        var installerPath = Directory.GetCurrentDirectory();
        var rootFolder = Directory.GetParent(installerPath)?.FullName;
        ArgumentNullException.ThrowIfNull(rootFolder, nameof(rootFolder));

        var appPath = Path.Combine(rootFolder, "app");

        return appPath;
    }

    public static string GetOutputPath(IConfiguration configuration)
    {
        var path = configuration["output"];
        if (!string.IsNullOrEmpty(path)) return path;

        return "app";
    }
}