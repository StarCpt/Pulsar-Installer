using Newtonsoft.Json;
using Pulsar.Shared;
using PulsarInstaller.GitHub;
using PulsarInstaller.ViewModels;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;

namespace PulsarInstaller;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string InstalledAppName { get; } = "Pulsar";
    public static string InstallerVersion { get; } = "2.0";

    const bool TEST_SILENT_UPGRADE = false;
    const string VERSION_FILE_NAME = "version.txt";
    const string LATEST_RELEASE_URL = "https://api.github.com/repos/SpaceGT/Pulsar/releases/latest";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (TEST_SILENT_UPGRADE || Environment.GetCommandLineArgs().Contains("-silent-upgrade"))
        {
            Task.Run(DoSilentUpgrade).Wait();
            this.Shutdown();
            return;
        }

        MainWindow = new MainWindow
        {
            DataContext = new MainViewModel(),
        };

        MainWindow.Show();
    }

    private static async Task DoSilentUpgrade()
    {
        try
        {
            // check for new Pulsar release, download, and unpack new files without user interaction
            // the installer exe is in the same directory as the launcher

            string pulsarDir = Path.GetDirectoryName(Environment.ProcessPath)!;

            if (TEST_SILENT_UPGRADE)
                pulsarDir = Path.Combine(pulsarDir, "upgrade_test");

            var installedVer = TryGetInstalledVersion(pulsarDir) ?? new SemanticVersion(0, 0, 0);
            var (latestVer, latestReleaseInfo) = await TryGetLatestVersion();

            if (latestVer.HasValue)
            {
                bool needsUpgrade = latestVer.Value > installedVer;
                if (!needsUpgrade)
                {
                    // up to date
                    return;
                }
            }
            else // if (!latestVer.HasValue)
            {
                // assume latest if can't get latest release version
                // supersedes missing installedVer
                ShowError("Update Failed: Could not fetch latest version info.");
                return;
            }

            ZipArchive? latestReleaseAssetZip = await DownloadReleaseAsset(latestReleaseInfo!);

            if (latestReleaseAssetZip is null)
            {
                ShowError("Update Failed: Could not download latest version.");
                return;
            }

            UnpackArchive(pulsarDir, latestReleaseAssetZip);
            SavePulsarVersion(pulsarDir, latestVer.Value);
            UpdateLibrariesFolderChecksum(pulsarDir);

            ShowError("Pulsar Updated!");
        }
        catch (Exception e)
        {
            ShowError("Update Failed: " + e);
        }
    }

    public static SemanticVersion? TryGetInstalledVersion(string pulsarDirectory)
    {
        string versionFilePath = Path.Combine(pulsarDirectory, VERSION_FILE_NAME);
        if (!File.Exists(versionFilePath))
            return null;

        // read current version from pulsarversion.txt file
        string currentVersionStr = File.ReadAllText(versionFilePath).Trim();
        return ParseVersionString(currentVersionStr);
    }

    public static async Task<(SemanticVersion? version, Release latestReleaseInfo)> TryGetLatestVersion()
    {
        try
        {
            // check latest github release version

            using var webClient = new HttpClient();
            webClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // required for github API

            // get api response json
            using var response = await webClient.GetStreamAsync(LATEST_RELEASE_URL);

            // deserialize json
            using var responseReader = new StreamReader(response);
            using var responseJsonReader = new JsonTextReader(responseReader);
            Release latestReleaseInfo = JsonSerializer.CreateDefault().Deserialize<Release>(responseJsonReader)!;

            // parse version from release name
            // the name of the release should be in SemVer format (v1.2.3)
            return (ParseVersionString(latestReleaseInfo.Name[1..]), latestReleaseInfo);
        }
        catch
        {
            return default; // assume latest if can't get latest release version
        }
    }

    public static async Task<ZipArchive?> DownloadReleaseAsset(Release releaseInfo)
    {
        // find first valid zip asset
        string assetEndsWithStr = releaseInfo.Name + ".zip"; // v1.2.3.zip
        var assetToDownload = releaseInfo.Assets.FirstOrDefault(i => i.Name.EndsWith(assetEndsWithStr, StringComparison.OrdinalIgnoreCase));

        if (assetToDownload is null)
            return null;

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // required for github API

        // download zip
        var response = await client.GetStreamAsync(assetToDownload.BrowserDownloadUrl);
        return new ZipArchive(response);
    }

    public static void UnpackArchive(string pulsarDirectory, ZipArchive archive)
    {
        archive.ExtractToDirectory(pulsarDirectory, true);
    }

    public static void SavePulsarVersion(string pulsarDirectory, SemanticVersion version)
    {
        File.WriteAllText(Path.Combine(pulsarDirectory, VERSION_FILE_NAME), $"v{version.Major}.{version.Minor}.{version.Patch}");
    }

    private static SemanticVersion ParseVersionString(string versionStr)
    {
        string[] versionStrArr = versionStr.Split('.');
        int majorVersion = versionStrArr.Length >= 1 && int.TryParse(versionStrArr[0], out int major) ? major : 0;
        int minorVersion = versionStrArr.Length >= 2 && int.TryParse(versionStrArr[1], out int minor) ? minor : 0;
        int patchVersion = versionStrArr.Length >= 3 && int.TryParse(versionStrArr[2], out int patch) ? patch : 0;
        return new SemanticVersion(majorVersion, minorVersion, patchVersion);
    }

    public static void UpdateLibrariesFolderChecksum(string pulsarDirectory)
    {
        string librariesDir = Path.Combine(pulsarDirectory, "Libraries");
        if (!Directory.Exists(librariesDir))
            return;
        string hash = Tools.GetFolderHash(librariesDir);
        File.WriteAllText(Path.Combine(pulsarDirectory, "checksum.txt"), hash);
    }

    public static void ShowError(string str)
    {
        // TODO: show some dialog
    }
}

