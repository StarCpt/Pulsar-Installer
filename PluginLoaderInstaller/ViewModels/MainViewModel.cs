using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace PluginLoaderInstaller.ViewModels;

public readonly record struct InstallOptions
{
    public required string Bin64Path { get; init; }
    public required bool AddLaunchOptions { get; init; }
    public required bool AddAsNonSteamGame { get; init; }
    public required bool SkipIntroFlag { get; init; }
}

public partial class MainViewModel : ObservableObject
{
    public string AppTitle { get; } = $"{App.InstalledAppName} Installer v{App.Version}";

    [ObservableProperty]
    public partial PageViewModelBase Page { get; private set; }

    private bool Installed => Page is InstallationPageViewModel { InstallProgress: >= 1.0 };

    private PageViewModelBase[] _pages;

    public MainViewModel()
    {
        _pages = [
            new LicensePageViewModel(this),
            new InstallOptionsPageViewModel(this),
            new InstallationPageViewModel(this),
        ];

        Page = _pages[0];
    }

    public InstallOptions GetInstallOptions()
    {
        var optionsPageVm = _pages.OfType<InstallOptionsPageViewModel>().Single();
        return new InstallOptions
        {
            Bin64Path = optionsPageVm.Bin64Path,
            AddLaunchOptions = optionsPageVm.AddLaunchOptions,
            AddAsNonSteamGame = optionsPageVm.AddAsNonSteamGame,
            SkipIntroFlag = optionsPageVm.SkipIntroFlag,
        };
    }

    [RelayCommand]
    public void PrevPage()
    {
        int pageIndex = Array.IndexOf(_pages, Page);
        if (pageIndex == 0)
            return;
        Page = _pages[pageIndex - 1];

    }

    [RelayCommand]
    public void NextPage()
    {
        if (Installed)
        {
            Cancel();
            return;
        }

        int pageIndex = Array.IndexOf(_pages, Page);
        if (pageIndex == (_pages.Length - 1))
            return;
        Page = _pages[pageIndex + 1];

        if (Page is InstallationPageViewModel installationPage)
        {
            installationPage.Install();
        }
    }

    [RelayCommand]
    public void Uninstall()
    {

    }

    [RelayCommand]
    public static void Cancel() => App.Current.Shutdown();
}
