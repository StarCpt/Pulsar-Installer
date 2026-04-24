using PulsarInstaller.ViewModels;
using System.Reflection;
using System.Windows;

namespace PulsarInstaller;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string InstalledAppName { get; } = "Pulsar";
    public static string InstallerVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Unknown Version";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        MainWindow = new MainWindow
        {
            DataContext = new MainViewModel(),
        };

        MainWindow.Show();
    }

}

