using Microsoft.Win32;
using System.Diagnostics;

namespace PluginLoaderInstaller;

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
}
