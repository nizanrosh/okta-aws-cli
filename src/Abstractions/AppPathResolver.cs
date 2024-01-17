using System.Runtime.InteropServices;
using Abstractions.Consts;

namespace Abstractions;

public class AppPathResolver
{
    public static string GetAppPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Constants.WindowsAppPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return Constants.LinuxAppPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return Constants.OsxAppPath;

        throw new ArgumentException("Could not determine OS.");
    }
}