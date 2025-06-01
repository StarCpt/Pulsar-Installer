using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using PluginLoaderInstaller.Models.Vdf;
using System.IO;

namespace PluginLoaderInstaller.ViewModels;

public partial class InstallOptionsPageViewModel(MainViewModel mainViewModel) : PageViewModelBase(mainViewModel)
{
    public override string Header => "Choose Installation Options";
    public override string Description => "Please select your Space Engineers Bin64 directory below.\nExample: C:\\Program Files (x86)\\Steam\\steamapps\\common\\SpaceEngineers\\Bin64";

    public override bool BackButtonEnabled => true;
    public override bool NextButtonEnabled => ValidateBin64Path(Bin64Path);
    public override string NextButtonText => "Install";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextButtonEnabled))]
    public partial string Bin64Path { get; set; } = TryFindBin64Path() ?? "";

    [ObservableProperty]
    public partial bool AddLaunchOptions { get; set; } = false;
    [ObservableProperty]
    public partial bool AddAsNonSteamGame { get; set; } = false;

    private static string? TryFindBin64Path()
    {
        string? steamInstallPath = SteamHelpers.TryGetSteamPath();

        if (steamInstallPath is null)
            return null;

        // read libraryfolders.vdf
        string libaryFoldersVdfPath = Path.Combine(steamInstallPath, "steamapps", "libraryfolders.vdf");

        if (!File.Exists(libaryFoldersVdfPath))
            return null;
        
        try
        {
            using var libraryFolderVdfReader = File.OpenText(libaryFoldersVdfPath);
            VProperty libraryFoldersData = VdfConvert.Deserialize(libraryFolderVdfReader);
            var libraryFolders = libraryFoldersData.Value.Select(i => ((VProperty)i).Value.ToJson().ToObject<LibraryFolder>());

            if (libraryFolders is null)
                return null;
            
            foreach (LibraryFolder? folder in libraryFolders)
            {
                if (folder?.path is null || (folder?.apps?.Count ?? 0) is 0)
                    continue;
            
                foreach (ulong appId in folder!.apps.Keys)
                {
                    string bin64Location = Path.Combine(folder.path, "steamapps", "common", "SpaceEngineers", "Bin64");
                    if (appId == 244850 && ValidateBin64Path(bin64Location))
                    {
                        return bin64Location;
                    }
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static bool ValidateBin64Path(string path)
    {
        if (!Path.IsPathRooted(path))
            return false;

        if (!Directory.Exists(path))
            return false;

        string gameExePath = Path.Combine(path, "SpaceEngineers.exe");
        if (!File.Exists(gameExePath))
            return false;

        return true;
    }

    [RelayCommand]
    public void BrowseFolders()
    {
        var dialog = new OpenFolderDialog();
        
        if (dialog.ShowDialog() ?? false && ValidateBin64Path(dialog.FolderName))
        {
            Bin64Path = dialog.FolderName;
        }
    }
}
