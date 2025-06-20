using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Windows;

namespace CustomWireGuardLauncher;

public partial class MainWindow : Window
{
    private readonly List<string> _configFiles = new();
    private readonly string _configDir;
    private const string ServiceName = "ProtonVPN WireGuard";
    private const string ConfigFilePath = "C:\\ProgramData\\ProtonVPN\\WireGuard\\ProtonVPN.conf";

    public MainWindow()
    {
        InitializeComponent();
        _configDir = Path.Combine(AppContext.BaseDirectory, "wireguard-configs");
        LoadConfigs();
    }

    private void LoadConfigs()
    {
        if (!Directory.Exists(_configDir))
        {
            Directory.CreateDirectory(_configDir);
        }
        _configFiles.Clear();
        _configFiles.AddRange(Directory.GetFiles(_configDir, "*.conf"));
        ConfigBox.ItemsSource = _configFiles.Select(Path.GetFileNameWithoutExtension);
        if (ConfigBox.Items.Count > 0)
        {
            ConfigBox.SelectedIndex = 0;
        }
    }

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        if (ConfigBox.SelectedIndex < 0) return;
        string configName = _configFiles[ConfigBox.SelectedIndex];
        try
        {
            string configText = await FetchConfigAsync();
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath)!);
            await File.WriteAllTextAsync(ConfigFilePath, configText);
            StartWireGuard(ConfigFilePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to connect: {ex.Message}");
        }
    }

    private void Disconnect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StopWireGuard();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to disconnect: {ex.Message}");
        }
    }

    private static void StartWireGuard(string configPath)
    {
        ServiceController sc = new(ServiceName);
        if (sc.Status != ServiceControllerStatus.Running)
        {
            sc.Start();
        }
    }

    private static void StopWireGuard()
    {
        ServiceController sc = new(ServiceName);
        if (sc.Status != ServiceControllerStatus.Stopped)
        {
            sc.Stop();
        }
    }

    private static async Task<string> FetchConfigAsync()
    {
        using HttpClient client = new();
        return await client.GetStringAsync("https://www.futuresoftware.ae/APITest/WgApi");
    }
}
