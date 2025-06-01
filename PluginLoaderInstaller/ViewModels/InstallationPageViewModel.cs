using VdfSharp;
using VdfSharp.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace PluginLoaderInstaller.ViewModels;

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

    private const bool SIMULATE_INSTALLATION = false; // doesn't close steam and doesn't actually unpack files to bin64

    public async void Install()
    {
        Log.Clear();

        InstallProgress = 0;
        WriteLog($"Installing {App.InstalledAppName} v{App.InstalledAppVersion}.");

        WriteLogNewline();
        WriteEnvironmentInfo();
        WriteLogNewline();

        try
        {
            InstallOptions options = MainViewModel.GetInstallOptions();

            bool needToCloseSteam = options.AddLaunchOptions || options.AddAsNonSteamGame || options.SkipIntroFlag;
            bool steamClosed = false;
            if (needToCloseSteam && SteamHelpers.IsSteamRunning())
            {
                steamClosed = true;
                WriteLog("One or more installation options selected requires Steam to be closed.");
                WriteLog("Waiting for Steam to exit.");

                if (!SIMULATE_INSTALLATION)
                {
                    await SteamHelpers.CloseSteamAsync(true);
                }
                else
                {
                    await Task.Delay(2000);
                }

                WriteLogNewline();
            }

            string embeddedResourcePath = String.Join('.', nameof(PluginLoaderInstaller), "Assets", "pluginloader.zip");
            Stream archiveStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourcePath) ?? throw new Exception("Installation Failed");
            using ZipArchive archive = new(archiveStream);

            double progressPerFile = 0.9 / archive.Entries.Count;
            int fileIndex = 1;
            foreach (var entry in archive.Entries)
            {
                // Skip if directory
                if (entry.FullName.EndsWith("/"))
                    continue;

                string filePath = Path.Combine(options.Bin64Path, entry.FullName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                WriteLog($"Extracting {entry.FullName}.");

                if (File.Exists(filePath))
                    WriteLog($" - {entry.FullName} already exists, overwriting.");

                if (!SIMULATE_INSTALLATION)
                {
                    entry.ExtractToFile(filePath, true);
                }

                InstallProgress = fileIndex * progressPerFile;
                fileIndex++;

                await Task.Delay(10);
            }

            WriteLogNewline();

            bool addNewline = options.AddLaunchOptions || options.AddAsNonSteamGame || options.SkipIntroFlag || steamClosed;
            if (options.AddLaunchOptions || options.SkipIntroFlag)
            {
                AddLaunchOptions(options.AddLaunchOptions, options.SkipIntroFlag);
            }

            if (options.AddAsNonSteamGame)
            {
                //AddAsNonSteamGame();
            }

            if (steamClosed)
            {
                WriteLog("Launching Steam.");
                if (!SIMULATE_INSTALLATION)
                {
                    SteamHelpers.LaunchSteam();
                }
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
        WriteLog($"Installer Version: {App.Version}");
        WriteLog($"Environment.OSVersion: {Environment.OSVersion}");
        WriteLog($"Environment.Is64BitOperatingSystem: {Environment.Is64BitOperatingSystem}");
        WriteLog($"Environment.Is64BitProcess: {Environment.Is64BitProcess}");
    }

    private void AddLaunchOptions(bool addLaunchOptions, bool skipIntroFlag)
    {
        string launchOptionsStr = "";
        if (addLaunchOptions)
        {
            WriteLog($"Adding {App.InstalledAppName} to Space Engineers launch options.");
            if (launchOptionsStr.Length != 0)
                launchOptionsStr += " ";
            launchOptionsStr += "\\\"SpaceEngineersLauncher\\\" %command%";
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
                .TryGetValue<VdfProperty>("Software")?
                .TryGetValue<VdfProperty>("Valve")?
                .TryGetValue<VdfProperty>("Steam")?
                .TryGetValue<VdfProperty>("apps")?
                .TryGetValue<VdfProperty>("244850");

            if (seProperties != null)
            {
                // Check if launch options are already set
                if (seProperties.TryGetValue("LaunchOptions") is VdfKeyValue kv &&
                    (!addLaunchOptions || kv.Value.Contains("\\\"SpaceEngineersLauncher\\\" %command%")) &&
                    (!skipIntroFlag || kv.Value.Contains("-skipintro")))
                    continue;

                seProperties["LaunchOptions"] = new VdfKeyValue
                {
                    Key = "LaunchOptions",
                    Value = launchOptionsStr,
                };

                if (!SIMULATE_INSTALLATION)
                {
                    // make backup
                    File.Copy(userLocalConfigPath, userLocalConfigPath + $".backup_{DateTime.Now:yyMMdd_hhmmss}");

                    // write modified file
                    VdfSerializer.Serialize(userLocalConfig, userLocalConfigPath);
                }
            }
        }
    }

    private void AddAsNonSteamGame()
    {
        WriteLog($"Adding {App.InstalledAppName} as non-steam game.");

        // read shortcuts.vdf
        string userDataDir = Path.Combine(SteamHelpers.TryGetSteamPath()!, "userdata");
        foreach (var shortcutConfigPath in Directory.GetDirectories(userDataDir, "*").Select(i => Path.Combine(i, "config", "shortcuts.vdf")))
        {
            if (!File.Exists(shortcutConfigPath))
                continue;

            var shortcutConfig = VdfSerializer.Deserialize(shortcutConfigPath);

        }

        throw new NotImplementedException();
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
