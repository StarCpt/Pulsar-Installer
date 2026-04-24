using PulsarInstaller.Models;
using System.IO;

namespace PulsarInstaller.ViewModels;

public partial class InstallOptionsPageViewModel(MainViewModel mainViewModel) : PageViewModelBase(mainViewModel)
{
    public override string Header => "Choose Installation Options";
    public override string Description => "To continue, click Next.";

    public override bool BackButtonEnabled => true;
    public override bool NextButtonEnabled => true;
    public override string NextButtonText => "Install";

    public InstallOptions InstallOptions => _options;

    private InstallOptions _options = new()
    {
        InstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pulsar"),
        VersionToInstall = PulsarVersion.NetCore,
        AddLaunchOptionsSE1 = false,
        AddLaunchOptionsSE2 = false,
        CreateDesktopShortcutSE1 = false,
        RemovePluginLoader = true,
    };

    public string InstallPath => _options.InstallPath;

    public bool InstallNetFramework
    {
        get => _options.VersionToInstall is PulsarVersion.NetFramework;
        set
        {
            if (SetProperty(ref _options.VersionToInstall, PulsarVersion.NetFramework))
            {
                OnPropertyChanged(nameof(InstallNetCore));
            }
        }
    }

    public bool InstallNetCore
    {
        get => _options.VersionToInstall is PulsarVersion.NetCore;
        set
        {
            if (SetProperty(ref _options.VersionToInstall, PulsarVersion.NetCore))
            {
                OnPropertyChanged(nameof(InstallNetFramework));
            }
        }
    }

    public bool AddLaunchOptionsSE1
    {
        get => _options.AddLaunchOptionsSE1;
        set => SetProperty(ref _options.AddLaunchOptionsSE1, value);
    }

    public bool AddLaunchOptionsSE2
    {
        get => _options.AddLaunchOptionsSE2;
        set => SetProperty(ref _options.AddLaunchOptionsSE2, value);
    }

    public bool CreateDesktopShortcutSE1
    {
        get => _options.CreateDesktopShortcutSE1;
        set => SetProperty(ref _options.CreateDesktopShortcutSE1, value);
    }

    public bool CreateDesktopShortcutSE2
    {
        get => _options.CreateDesktopShortcutSE2;
        set => SetProperty(ref _options.CreateDesktopShortcutSE2, value);
    }

    public bool RemovePluginLoader
    {
        get => _options.RemovePluginLoader;
        set => SetProperty(ref _options.RemovePluginLoader, value);
    }
}
