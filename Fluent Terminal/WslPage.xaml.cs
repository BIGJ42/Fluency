using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluent_Terminal
{
    public sealed partial class WslPage : Page
    {
        public WslPage()
        {
            this.InitializeComponent();
            _ = CheckWslStatus();
        }

        private async Task CheckWslStatus()
        {
            try
            {
                LoadingProgress.Visibility = Visibility.Visible;
                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo("wsl.exe", "--status")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (Process p = Process.Start(psi))
                    {
                        if (p != null)
                        {
                            p.WaitForExit(5000);
                            this.DispatcherQueue.TryEnqueue(async () =>
                            {
                                WslStatus.Title = "WSL is Active";
                                WslStatus.Severity = InfoBarSeverity.Success;
                                await LoadInstalledDistros();
                            });
                        }
                    }
                });
            }
            catch (Exception)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    WslStatus.Title = "WSL not found";
                    WslStatus.Severity = InfoBarSeverity.Error;
                    LoadingProgress.Visibility = Visibility.Collapsed;
                });
            }
        }

        private async Task LoadInstalledDistros()
        {
            try
            {
                LoadingProgress.Visibility = Visibility.Visible;
                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo("wsl.exe", "-l -q")
                    {
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        StandardOutputEncoding = Encoding.Unicode
                    };

                    using (Process p = Process.Start(psi))
                    {
                        if (p != null)
                        {
                            string output = p.StandardOutput.ReadToEnd();
                            var distros = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(d => !string.IsNullOrWhiteSpace(d))
                                .ToList();

                            this.DispatcherQueue.TryEnqueue(() =>
                            {
                                DistroList.ItemsSource = distros;
                                LoadingProgress.Visibility = Visibility.Collapsed;
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    WslStatus.Title = $"Error loading distros: {ex.Message}";
                    WslStatus.Severity = InfoBarSeverity.Error;
                    LoadingProgress.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void ListDistros_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("powershell.exe", "-Command \"wsl --list --online | Out-GridView\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                WslStatus.Title = $"Error: {ex.Message}";
                WslStatus.Severity = InfoBarSeverity.Error;
            }
        }

        private void InstallWSL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("wsl.exe", "--install") { Verb = "runas", UseShellExecute = true });
            }
            catch (Exception ex)
            {
                WslStatus.Title = $"Error: {ex.Message}";
                WslStatus.Severity = InfoBarSeverity.Error;
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadInstalledDistros();
        }

        private void RemoveDistro_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var distro = button?.Tag as string;
            if (!string.IsNullOrEmpty(distro))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("wsl.exe", $"--unregister {distro}") { Verb = "runas", UseShellExecute = true });
                    _ = Task.Delay(2000).ContinueWith(_ => Refresh_Click(null, null));
                }
                catch (Exception ex)
                {
                    WslStatus.Title = $"Error unregistering distro: {ex.Message}";
                    WslStatus.Severity = InfoBarSeverity.Error;
                }
            }
        }
    }
}