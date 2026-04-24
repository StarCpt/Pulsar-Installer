using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using PulsarInstaller.Vdf;
using System.Diagnostics;
using System.IO;

namespace PulsarInstaller;

public static class SteamHelpers
{
    public static string? TryGetSteamPath()
    {
        string? path = (string?)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", null);
        return !String.IsNullOrWhiteSpace(path) ? path : null;
    }

    public static string? TryGetSteamExePath()
    {
        string? path = (string?)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamExe", null);
        return !String.IsNullOrWhiteSpace(path) ? path : null;
    }

    public static int? TryGetSteamProcessId()
    {
        return (int?)Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam\ActiveProcess", "pid", null);
    }

    private const int CLOSE_WAIT_MS = 200;

    public static void CloseSteam(bool waitUntilClosed)
    {
        CloseSteamInternal();

        if (!waitUntilClosed)
            return;

        while (true)
        {
            if (!IsSteamRunning())
                break;

            Thread.Sleep(CLOSE_WAIT_MS);
        }
    }

    public static async Task CloseSteamAsync(bool waitUntilClosed)
    {
        CloseSteamInternal();

        if (!waitUntilClosed)
            return;

        while (true)
        {
            if (!IsSteamRunning())
                break;

            await Task.Delay(CLOSE_WAIT_MS);
        }
    }

    private static void CloseSteamInternal()
    {
        string steamUrl = $"steam://exit";
        Process.Start(new ProcessStartInfo
        {
            FileName = steamUrl,
            UseShellExecute = true,
        });
    }

    public static bool IsSteamRunning()
    {
        Process[] steamProcesses = Process.GetProcessesByName("steam");
        Process[] steamServicesProcesses = Process.GetProcessesByName("steamservice");
        return steamProcesses.Union(steamServicesProcesses).Any(i =>
        {
            // returns if its still running
            try { return !i.HasExited; }
            catch { return true; }
        });

        //int? pid = TryGetSteamProcessId();
        //if (!pid.HasValue || pid.Value == 0)
        //{
        //    return false;
        //}
        //
        //try
        //{
        //    if (Process.GetProcessById(pid.Value).HasExited)
        //        return false;
        //}
        //catch
        //{
        //    return false;
        //}
        //
        //return true;
    }

    public static void LaunchSteam()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = TryGetSteamExePath(),
            WorkingDirectory = TryGetSteamPath(),
            UseShellExecute = true,
        });
    }

    public static string? TryFindSE1Bin64Path()
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

        static bool ValidateBin64Path(string path)
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
}
