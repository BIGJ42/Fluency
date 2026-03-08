using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Fluency
{
    public sealed partial class MaintenancePage : Page
    {
        public MaintenancePage()
        {
            this.InitializeComponent();
            Log("Maintenance center ready.");
        }

        private void Log(string message)
        {
            StatusLog.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            // Scroll to end if needed (not supported simple in TextBox but acceptable here)
        }

        private async void CleanupTempFiles_Click(object sender, RoutedEventArgs e)
        {
            Log("Starting temporary files cleanup...");
            try
            {
                await RunPowerShellAsync("Remove-Item -Path $env:TEMP\\* -Recurse -Force -ErrorAction SilentlyContinue");
                await RunPowerShellAsync("Remove-Item -Path 'C:\\Windows\\Temp\\*' -Recurse -Force -ErrorAction SilentlyContinue");
                Log("Temporary files cleaned successfully.");
            }
            catch (Exception ex)
            {
                Log($"Error during cleanup: {ex.Message}");
            }
        }

        private async void FlushDNS_Click(object sender, RoutedEventArgs e)
        {
            Log("Flushing DNS cache...");
            try
            {
                await RunPowerShellAsync("ipconfig /flushdns");
                Log("DNS cache flushed successfully.");
            }
            catch (Exception ex)
            {
                Log($"Error flushing DNS: {ex.Message}");
            }
        }

        private async void RestartExplorer_Click(object sender, RoutedEventArgs e)
        {
            Log("Restarting Windows Explorer...");
            try
            {
                await RunPowerShellAsync("taskkill /f /im explorer.exe; start explorer.exe");
                Log("Windows Explorer restarted.");
            }
            catch (Exception ex)
            {
                Log($"Error restarting explorer: {ex.Message}");
            }
        }

        private async void RebuildIconCache_Click(object sender, RoutedEventArgs e)
        {
            Log("Rebuilding icon cache...");
            try
            {
                string script = @"
taskkill /f /im explorer.exe
cd /d %userprofile%\AppData\Local\Microsoft\Windows\Explorer
attrib -h iconcache_*.db
del iconcache_*.db
start explorer.exe
";
                await RunPowerShellAsync(script);
                Log("Icon cache rebuilt. Explorer restarted.");
            }
            catch (Exception ex)
            {
                Log($"Error rebuilding icon cache: {ex.Message}");
            }
        }

        private async Task RunPowerShellAsync(string command)
        {
            await Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command.Replace("\"", "\\\"")}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas" // Elevate if possible
                };
                
                using (var process = Process.Start(psi))
                {
                    process?.WaitForExit();
                }
            });
        }
    }
}
