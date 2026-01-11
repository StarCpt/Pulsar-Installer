using PulsarInstaller.Models;

namespace PulsarInstaller.ViewModels;

public struct InstallOptions
{
    public string InstallPath;
    public PulsarVersion VersionToInstall;
    public bool AddLaunchOptions;
    public bool CreateDesktopShortcut;
    public bool RemovePluginLoader;
}
