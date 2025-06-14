﻿using PulsarInstaller.ViewModels;
using System.Windows;

namespace PulsarInstaller;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static string InstalledAppName { get; } = "Pulsar";
    public static string InstalledAppVersion { get; } = "1.13.0";
    public static string Version { get; } = "1.0";

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

