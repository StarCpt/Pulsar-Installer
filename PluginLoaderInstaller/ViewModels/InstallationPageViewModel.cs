using VdfSharp;
using VdfSharp.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Media;

namespace PulsarInstaller.ViewModels;

public partial class InstallationPageViewModel(MainViewModel mainViewModel) : PageViewModelBase(mainViewModel)
{
    private static readonly Brush _defaultProgressBarBrush = new SolidColorBrush(Color.FromRgb(6, 176, 37));
    private static readonly Brush _errorProgressBarBrush = new SolidColorBrush(Color.FromRgb(230, 0, 0));

    public override string Header => $"Installing {App.InstalledAppName}";
    public override string Description => "";

    public override bool BackButtonEnabled => false;
    public override bool NextButtonEnabled => InstallProgress >= 1.0;
    public override string NextButtonText => "Close";

    [ObservableProperty]
    public partial Brush ProgressBarBrush { get; private set; } = _defaultProgressBarBrush;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextButtonEnabled))]
    public partial double? InstallProgress { get; private set; } = 0;
    public StringBuilder Log { get; } = new StringBuilder();

    public async void Install()
    {
        Log.Clear();

        InstallProgress = 0;
        WriteLog($"Installing {App.InstalledAppName}.");

        WriteLog("Fetching version info.");

        // get latest version info
        var (latestVer, latestReleaseInfo) = await App.TryGetLatestVersion();
        if (!latestVer.HasValue)
        {
            throw new Exception("Could not fetch latest version.");
        }

        WriteLog($"Latest Version: v{latestVer.Value.Major}.{latestVer.Value.Minor}.{latestVer.Value.Patch}");

        WriteLogNewline();
        WriteEnvironmentInfo();
        WriteLogNewline();

        try
        {
            InstallOptions options = MainViewModel.GetInstallOptions();

            bool needToCloseSteam = options.AddLaunchOptions || options.SkipIntroFlag;
            bool steamClosed = false;
            if (needToCloseSteam && SteamHelpers.IsSteamRunning())
            {
                steamClosed = true;
                WriteLog("One or more installation options selected requires Steam to be closed.");
                WriteLog("Waiting for Steam to exit.");

                await SteamHelpers.CloseSteamAsync(true);

                WriteLogNewline();
            }

            if (options.RemovePluginLoader)
            {
                RemovePluginLoaderFiles(options.Bin64Path);
            }

            // download latest release asset zip
            WriteLog($"Downloading release v{latestVer.Value.Major}.{latestVer.Value.Minor}.{latestVer.Value.Patch}");
            ZipArchive? archive = await App.DownloadReleaseAsset(latestReleaseInfo);

            if (archive is null)
            {
                throw new Exception("Could not download latest version.");
            }

            string pulsarDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), "Pulsar");

            double progressPerFile = 0.9 / archive.Entries.Count;
            int fileIndex = 1;
            foreach (var entry in archive.Entries)
            {
                // Skip if directory
                if (entry.FullName.EndsWith('/'))
                    continue;

                string filePath = Path.Combine(pulsarDirectory, entry.FullName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                WriteLog($"Extracting {entry.FullName}.");

                if (File.Exists(filePath))
                    WriteLog($" - {entry.FullName} already exists, overwriting.");

                entry.ExtractToFile(filePath, true);

                InstallProgress = fileIndex * progressPerFile;
                fileIndex++;

                await Task.Delay(10);
            }

            WriteLogNewline();

            bool addNewline = options.AddLaunchOptions || options.SkipIntroFlag || steamClosed;
            if (options.AddLaunchOptions || options.SkipIntroFlag)
            {
                AddLaunchOptions(options.AddLaunchOptions, options.SkipIntroFlag, Path.Combine(pulsarDirectory, "Legacy.exe"));
            }

            if (steamClosed)
            {
                WriteLog("Launching Steam.");
                SteamHelpers.LaunchSteam();
            }

            if (addNewline)
                WriteLogNewline();
        }
        catch (Exception e)
        {
            ProgressBarBrush = _errorProgressBarBrush;
            WriteLog(e.ToString());
            WriteLog("An error occurred during installation");
            return;
        }

        InstallProgress = 1.0;
        WriteLog("Installation completed.");
    }

    private void WriteEnvironmentInfo()
    {
        WriteLog($"Installer Version: {App.InstallerVersion}");
        WriteLog($"Environment.OSVersion: {Environment.OSVersion}");
        WriteLog($"Environment.Is64BitOperatingSystem: {Environment.Is64BitOperatingSystem}");
        WriteLog($"Environment.Is64BitProcess: {Environment.Is64BitProcess}");
    }

    private void AddLaunchOptions(bool addLaunchOptions, bool skipIntroFlag, string pulsarExePath)
    {
        string launchOptionsStr = "";
        if (addLaunchOptions)
        {
            WriteLog($"Adding {App.InstalledAppName} to Space Engineers launch options.");
            if (launchOptionsStr.Length != 0)
                launchOptionsStr += " ";
            launchOptionsStr += $"\\\"{pulsarExePath.Replace("\\", "\\\\")}\\\" %command%";
        }

        if (skipIntroFlag)
        {
            WriteLog($"Adding -skipintro to Space Engineers launch options.");
            if (launchOptionsStr.Length != 0)
                launchOptionsStr += " ";
            launchOptionsStr += "-skipintro";
        }

        // read localconfigs.vdf
        string userDataDir = Path.Combine(SteamHelpers.TryGetSteamPath()!, "userdata");
        foreach (var userLocalConfigPath in Directory.GetDirectories(userDataDir, "*").Select(i => Path.Combine(i, "config", "localconfig.vdf")))
        {
            if (!File.Exists(userLocalConfigPath))
                continue;

            var userLocalConfig = VdfSerializer.Deserialize(userLocalConfigPath);
            var seProperties = userLocalConfig
                .TryGetValue<VdfProperty>("Software", StringComparison.OrdinalIgnoreCase)?
                .TryGetValue<VdfProperty>("Valve", StringComparison.OrdinalIgnoreCase)?
                .TryGetValue<VdfProperty>("Steam", StringComparison.OrdinalIgnoreCase)?
                .TryGetValue<VdfProperty>("apps", StringComparison.OrdinalIgnoreCase)?
                .TryGetValue<VdfProperty>("244850");

            if (seProperties != null)
            {
                seProperties["LaunchOptions"] = new VdfKeyValue
                {
                    Key = "LaunchOptions",
                    Value = launchOptionsStr,
                };

                // make backup
                File.Copy(userLocalConfigPath, userLocalConfigPath + $".backup_{DateTime.Now:yyMMdd_hhmmss}");

                // write modified file
                VdfSerializer.Serialize(userLocalConfig, userLocalConfigPath);
            }
        }
    }

    private void RemovePluginLoaderFiles(string bin64Path)
    {
        bool pluginLoaderInstalled = File.Exists(Path.Combine(bin64Path, "PluginLoader.dll"));
        if (pluginLoaderInstalled)
        {
            WriteLog("Removing existing Plugin Loader installation.");

            string[] pluginLoaderFiles =
            {
                "PluginLoader.dll",
                "0Harmony.dll",
                "Newtonsoft.Json.dll",
                "NuGet.Common.dll",
                "NuGet.Configuration.dll",
                "NuGet.Frameworks.dll",
                "NuGet.Packaging.dll",
                "NuGet.Protocol.dll",
                "NuGet.Resolver.dll",
                "NuGet.Versioning.dll",
            };

            foreach (var file in pluginLoaderFiles)
            {
                string filePath = Path.Combine(bin64Path, file);
                if (File.Exists(filePath))
                {
                    WriteLog($"Deleting {file}");

                    File.Delete(filePath);
                }
            }

            WriteLogNewline();
        }
    }

    private void WriteLog(string str)
    {
        Log.AppendLine(str);
        OnPropertyChanged(nameof(Log));
    }

    private void WriteLogNewline()
    {
        Log.AppendLine();
        OnPropertyChanged(nameof(Log));
    }
}
