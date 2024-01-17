using System.Runtime.InteropServices;
using Installer.Installers.Interfaces;

namespace Installer.Installers;

public class InstallersFactory : IInstallersFactory
{
    public IInstaller GetInstaller()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new WindowsInstaller();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return new LinuxInstaller();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return new OsxInstaller();

        throw new ArgumentException("Could not determine OS.");
    }
}