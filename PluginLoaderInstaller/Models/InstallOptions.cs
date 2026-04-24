using PulsarInstaller.Models;

namespace PulsarInstaller.ViewModels;

public struct InstallOptions
{
    public required string InstallPath;
    public required PulsarVersion VersionToInstall;
    public required bool AddLaunchOptionsSE1;
    public required bool AddLaunchOptionsSE2;
    public required bool CreateDesktopShortcutSE1;
    public required bool CreateDesktopShortcutSE2;
    public required bool RemovePluginLoader;
}
