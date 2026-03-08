using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fluent_Terminal
{
    public sealed partial class StartupAppsPage : Page
    {
        public StartupAppsPage()
        {
            this.InitializeComponent();
            _ = LoadStartupApps();
        }

        private async Task LoadStartupApps()
        {
            try
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    LoadingBar.Visibility = Visibility.Visible;
                    RefreshButton.IsEnabled = false;
                    AppCountText.Text = "Loading startup applications...";
                });

                await Task.Run(() =>
                {
                    var apps = new List<StartupApp>();

                    try
                    {
                        var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                        if (regKey != null)
                        {
                            foreach (var valueName in regKey.GetValueNames())
                            {
                                var value = regKey.GetValue(valueName)?.ToString();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    apps.Add(new StartupApp
                                    {
                                        AppName = valueName,
                                        AppPath = value,
                                        Source = "Registry (HKCU)",
                                        IsEnabled = true
                                    });
                                }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        string startupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Startup");
                        if (Directory.Exists(startupPath))
                        {
                            foreach (var file in Directory.GetFiles(startupPath))
                            {
                                apps.Add(new StartupApp
                                {
                                    AppName = Path.GetFileNameWithoutExtension(file),
                                    AppPath = file,
                                    Source = "Startup Folder",
                                    IsEnabled = true
                                });
                            }
                        }
                    }
                    catch { }

                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        StartupList.ItemsSource = apps.OrderBy(a => a.AppName).ToList();
                        AppCountText.Text = $"Found {apps.Count} startup application{(apps.Count != 1 ? "s" : "")}";
                        LoadingBar.Visibility = Visibility.Collapsed;
                        RefreshButton.IsEnabled = true;
                    });
                });
            }
            catch (Exception ex)
            {
                this.DispatcherQueue.TryEnqueue(async () =>
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                    RefreshButton.IsEnabled = true;
                    await new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to load startup apps: {ex.Message}",
                        PrimaryButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                });
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadStartupApps();
        }

        private void OnAppToggled(object sender, RoutedEventArgs e)
        {
            // Implementation for toggling startup apps (future enhancement)
        }

        private async void RemoveApp_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is StartupApp app)
            {
                var result = await new ContentDialog
                {
                    Title = "Remove Startup App",
                    Content = $"Are you sure you want to remove '{app.AppName}' from startup?",
                    PrimaryButtonText = "Remove",
                    SecondaryButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    try
                    {
                        if (app.Source == "Registry (HKCU)")
                        {
                            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                            regKey?.DeleteValue(app.AppName, false);
                        }
                        else if (app.Source == "Startup Folder" && File.Exists(app.AppPath))
                        {
                            File.Delete(app.AppPath);
                        }

                        await LoadStartupApps();
                    }
                    catch (Exception ex)
                    {
                        await new ContentDialog
                        {
                            Title = "Error",
                            Content = $"Failed to remove app: {ex.Message}",
                            PrimaryButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        }.ShowAsync();
                    }
                }
            }
        }
    }

    public class StartupApp
    {
        public required string AppName { get; set; }
        public required string AppPath { get; set; }
        public required string Source { get; set; }
        public bool IsEnabled { get; set; }
    }
}
