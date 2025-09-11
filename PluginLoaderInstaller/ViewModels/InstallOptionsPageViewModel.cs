using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace PulsarInstaller.ViewModels;

public partial class InstallOptionsPageViewModel(MainViewModel mainViewModel) : PageViewModelBase(mainViewModel)
{
    public override string Header => "Choose Installation Options";
    public override string Description => "To continue, click Next.";

    public override bool BackButtonEnabled => true;
    public override bool NextButtonEnabled => true;
    public override string NextButtonText => "Install";

    [ObservableProperty]
    public partial string InstallPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pulsar");
    [ObservableProperty]
    public partial bool AddLaunchOptions { get; set; } = false;
    [ObservableProperty]
    public partial bool RemovePluginLoader { get; set; } = true;
}
