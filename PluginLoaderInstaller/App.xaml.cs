using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Gameloop.Vdf.Linq;
using PulsarInstaller.Vdf;
using PulsarInstaller.ViewModels;
using System.IO;
using System.Windows;

namespace PulsarInstaller;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string InstalledAppName { get; } = "Pulsar";
    public static string InstallerVersion { get; } = "2.0.2";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        MainWindow = new MainWindow
        {
            DataContext = new MainViewModel(),
        };

        MainWindow.Show();
    }

    public static string? TryFindBin64Path()
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

}

