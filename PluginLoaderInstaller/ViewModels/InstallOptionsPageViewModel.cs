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
        VersionToInstall = PulsarVersion.NetFramework,
        AddLaunchOptions = false,
        CreateDesktopShortcut = false,
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

    public bool AddLaunchOptions
    {
        get => _options.AddLaunchOptions;
        set => SetProperty(ref _options.AddLaunchOptions, value);
    }

    public bool CreateDesktopShortcut
    {
        get => _options.CreateDesktopShortcut;
        set => SetProperty(ref _options.CreateDesktopShortcut, value);
    }

    public bool RemovePluginLoader
    {
        get => _options.RemovePluginLoader;
        set => SetProperty(ref _options.RemovePluginLoader, value);
    }
}
