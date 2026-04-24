using PulsarInstaller.Models;

namespace PulsarInstaller.ViewModels;

public struct InstallOptions
{
    public string InstallPath;
    public PulsarVersion VersionToInstall;
    public bool AddLaunchOptionsSE1;
    public bool AddLaunchOptionsSE2;
    public bool CreateDesktopShortcutSE1;
    public bool CreateDesktopShortcutSE2;
    public bool RemovePluginLoader;
}
