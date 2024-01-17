namespace Installer.Installers.Interfaces;

public interface IInstallersFactory
{
    IInstaller GetInstaller();
}