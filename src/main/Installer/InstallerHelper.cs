using System.Diagnostics;

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
}