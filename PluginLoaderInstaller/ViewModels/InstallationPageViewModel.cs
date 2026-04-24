using CommunityToolkit.Mvvm.ComponentModel;
using PulsarInstaller.Com;
using PulsarInstaller.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Media;
using VdfSharp;
using VdfSharp.Data;

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

        try
        {
            WriteLog("Fetching version info.");

            // get latest version info
            var (latestVer, latestReleaseInfo) = await InstallHelpers.TryGetLatestVersion();
            if (!latestVer.HasValue)
            {
                throw new Exception("Could not fetch latest version.");
            }

            WriteLog($"Latest Version: v{latestVer.Value.Major}.{latestVer.Value.Minor}.{latestVer.Value.Patch}");

            WriteLogNewline();
            WriteEnvironmentInfo();
            WriteLogNewline();

            InstallOptions options = MainViewModel.GetInstallOptions();

            bool needToCloseSteam = options.AddLaunchOptionsSE1 || options.AddLaunchOptionsSE2;
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
                TryRemovePluginLoaderFiles();
            }

            // download latest release asset zip
            WriteLog($"Downloading release v{latestVer.Value.Major}.{latestVer.Value.Minor}.{latestVer.Value.Patch}");
            ZipArchive? archive = await InstallHelpers.DownloadReleaseAsset(latestReleaseInfo);

            if (archive is null)
            {
                throw new Exception("Could not download latest version.");
            }

            string pulsarDirectory = options.InstallPath;

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

            bool addNewline = options.AddLaunchOptionsSE1 || options.AddLaunchOptionsSE2 || options.CreateDesktopShortcutSE1 || options.CreateDesktopShortcutSE2 || steamClosed;

            string pulsarExePathSE1 = Path.Combine(pulsarDirectory, options.VersionToInstall is PulsarVersion.NetFramework ? "Legacy.exe" : "Interim.exe");
            string pulsarExePathSE2 = Path.Combine(pulsarDirectory, "Modern.exe");

            if (options.AddLaunchOptionsSE1 || options.AddLaunchOptionsSE2)
            {
                if (options.AddLaunchOptionsSE1)
                {
                    WriteLog($"Adding Pulsar to Space Engineeers launch options.");
                }

                if (options.AddLaunchOptionsSE2)
                {
                    WriteLog($"Adding Pulsar to Space Engineeers 2 launch options.");
                }

                var configs = LoadUserLocalConfigs();
                foreach (var conf in configs)
                {
                    try
                    {
                        bool modified = false;

                        if (options.AddLaunchOptionsSE1)
                        {
                            const string APPID_SE1 = "244850";
                            string launchOptionsStrSE1 = $"\\\"{pulsarExePathSE1.Replace(@"\", @"\\")}\\\" %command%";

                            modified = conf.AddGameLaunchOptions(APPID_SE1, launchOptionsStrSE1);
                        }

                        if (options.AddLaunchOptionsSE2)
                        {
                            const string APPID_SE2 = "1133870";
                            string launchOptionsStrSE2 = $"\\\"{pulsarExePathSE2.Replace(@"\", @"\\")}\\\" %command%";

                            modified = conf.AddGameLaunchOptions(APPID_SE2, launchOptionsStrSE2);
                        }

                        if (modified)
                        {
                            conf.Save(true);
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLog($"An error occurred while adding launch options, skipping. File={conf.FilePath}, Error={e}");
                    }
                }
            }

            if (options.CreateDesktopShortcutSE1)
            {
                CreateDesktopShortcut(pulsarDirectory, pulsarExePathSE1, "Pulsar for SE");
            }

            if (options.CreateDesktopShortcutSE2)
            {
                CreateDesktopShortcut(pulsarDirectory, pulsarExePathSE2, "Pulsar for SE2");
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

    enum GameVersionEnum
    {
        SpaceEngineers1,
        SpaceEngineers2,
    }

    class UserLocalConfig
    {
        public string FilePath => _filePath;

        private readonly VdfProperty _root;
        private readonly string _filePath;

        private UserLocalConfig(VdfProperty root, string filePath)
        {
            _root = root;
            _filePath = filePath;
        }

        public bool AddGameLaunchOptions(string appId, string launchOptionsStr)
        {
            var appProperties = _root
                .TryGetValue<VdfProperty>("Software", StringComparison.OrdinalIgnoreCase)?
                .TryGetValue<VdfProperty>("Valve", StringComparison.OrdinalIgnoreCase)?
                .TryGetValue<VdfProperty>("Steam", StringComparison.OrdinalIgnoreCase)?
                .TryGetValue<VdfProperty>("apps", StringComparison.OrdinalIgnoreCase)?
                .TryGetValue<VdfProperty>(appId);

            if (appProperties is null)
                return false;

            // Check if launch options are already set
            if (appProperties.TryGetValue("LaunchOptions") is VdfKeyValue kv && kv.Value.Contains(launchOptionsStr))
                return false;

            appProperties["LaunchOptions"] = new VdfKeyValue
            {
                Key = "LaunchOptions",
                Value = launchOptionsStr,
            };

            return true;
        }

        private void CreateBackup()
        {
            string backupFileName = _filePath + ".backup";
            if (File.Exists(backupFileName))
            {
                if (File.Exists(backupFileName + "_1"))
                {
                    if (File.Exists(backupFileName + "_2"))
                    {
                        File.Move(backupFileName + "_2", backupFileName + "_3", true);
                    }
                    File.Move(backupFileName + "_1", backupFileName + "_2", true);
                }
                File.Move(backupFileName, backupFileName + "_1", true);
            }

            // make backup
            File.Copy(_filePath, backupFileName, true);
        }

        public void Save(bool createBackup)
        {
            if (createBackup)
            {
                CreateBackup();
            }
            VdfSerializer.Serialize(_root, _filePath);
        }

        public static bool TryDeserialize(string filePath, [NotNullWhen(true)] out UserLocalConfig? result)
        {
            result = null;

            if (!File.Exists(filePath))
                return false;

            try
            {
                var root = VdfSerializer.Deserialize(filePath);

                // basic validation to make sure we didn't deserialize a random file
                bool isUserLocalConfig = root
                    .TryGetValue<VdfProperty>("Software", StringComparison.OrdinalIgnoreCase)?
                    .TryGetValue<VdfProperty>("Valve", StringComparison.OrdinalIgnoreCase)?
                    .TryGetValue<VdfProperty>("Steam", StringComparison.OrdinalIgnoreCase) != null;

                if (!isUserLocalConfig)
                    return false;

                result = new UserLocalConfig(root, filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    static List<UserLocalConfig> LoadUserLocalConfigs()
    {
        List<UserLocalConfig> result = [];

        // read localconfigs.vdf
        string userDataDir = Path.Combine(SteamHelpers.TryGetSteamPath()!, "userdata");
        foreach (var userLocalConfigPath in Directory.GetDirectories(userDataDir, "*").Select(i => Path.Combine(i, "config", "localconfig.vdf")))
        {
            if (UserLocalConfig.TryDeserialize(userLocalConfigPath, out var config))
            {
                result.Add(config);
            }
        }

        return result;
    }

    private void CreateDesktopShortcut(string pulsarDirectory, string pulsarExePath, string shortcutName)
    {
        WriteLog("Creating desktop shortcut.");

        try
        {
            // https://stackoverflow.com/a/14632782/17308456

            IShellLink link = (IShellLink)new ShellLink();
            link.SetPath(pulsarExePath);
            link.SetWorkingDirectory(pulsarDirectory);

            IPersistFile file = (IPersistFile)link;
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolderOption.Create);

            if (!string.IsNullOrEmpty(desktopPath))
            {
                string shortcutFilePath = Path.Combine(desktopPath, $"{shortcutName}.lnk");

                // delete existing shortcut
                if (File.Exists(shortcutFilePath))
                {
                    File.Delete(shortcutFilePath);
                }

                file.Save(shortcutFilePath, false);
            }
        }
        catch (Exception e)
        {
            WriteLog(e.ToString());
            WriteLog("An error occurred while creating desktop shortcut");
        }
    }

    private void TryRemovePluginLoaderFiles()
    {
        string? bin64Path = SteamHelpers.TryFindSE1Bin64Path();
        if (string.IsNullOrEmpty(bin64Path))
            return;

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
