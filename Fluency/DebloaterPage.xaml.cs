using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Fluency
{
    public sealed partial class DebloaterPage : Page
    {
        private ObservableCollection<string> _bloatwareApps = new();

        public ObservableCollection<string> BloatwareApps => _bloatwareApps;

        public DebloaterPage()
        {
            this.InitializeComponent();
            InitializeBloatwareList();
        }

        private void InitializeBloatwareList()
        {
            _bloatwareApps.Add("Cortana");
            _bloatwareApps.Add("Microsoft Tips");
            _bloatwareApps.Add("Get Help");
            _bloatwareApps.Add("Feedback Hub");
            _bloatwareApps.Add("Microsoft Solitaire Collection");
            _bloatwareApps.Add("Candy Crush");
            _bloatwareApps.Add("Microsoft News");
            _bloatwareApps.Add("Xbox App");
        }

        private async void RemoveBloatware_Click(object sender, RoutedEventArgs e)
        {
            OperationLog.Text = "Starting bloatware removal process...";
            OperationProgress.Value = 0;
            OperationProgress.Visibility = Visibility.Visible;
            await ExecuteRemovalProcessAsync();
        }

        private async void RemoveOneDrive_Click(object sender, RoutedEventArgs e)
        {
            OperationLog.Text = "Uninstalling OneDrive...";
            OperationProgress.IsIndeterminate = true;
            OperationProgress.Visibility = Visibility.Visible;
            await ExecuteOneDriveRemovalAsync();
            OperationProgress.IsIndeterminate = false;
            OperationProgress.Visibility = Visibility.Collapsed;
        }

        private async void OptimizeSystem_Click(object sender, RoutedEventArgs e)
        {
            OperationLog.Text = "Optimizing system...";
            OperationProgress.Value = 0;
            OperationProgress.Visibility = Visibility.Visible;
            await ExecuteOptimizationAsync();
        }

        private async Task ExecuteRemovalProcessAsync()
        {
            try
            {
                var appsToRemove = new[]
                {
                    "Microsoft.MicrosoftTips",
                    "Microsoft.GetHelp",
                    "Microsoft.Getstarted",
                    "Microsoft.ZuneMusic",
                    "Microsoft.ZuneVideo",
                    "Microsoft.BingNews",
                    "Microsoft.BingWeather",
                    "Microsoft.BingSports",
                    "Microsoft.YourPhone",
                    "Microsoft.People",
                    "Microsoft.WindowsMaps",
                    "Microsoft.MicrosoftSolitaireCollection",
                    "microsoft.windowscommunicationsapps",
                    "Microsoft.SkypeApp",
                    "Microsoft.MixedReality.Portal",
                    "Microsoft.XboxApp",
                    "Microsoft.XboxGamingOverlay",
                    "Microsoft.XboxIdentityProvider",
                    "Microsoft.XboxSpeechToTextOverlay"
                };

                double progressStep = 100.0 / appsToRemove.Length;

                for (int i = 0; i < appsToRemove.Length; i++)
                {
                    var app = appsToRemove[i];
                    OperationLog.Text += $"\nRemoving {app}...";
                    await RunCommandAsync($"powershell -Command \"Get-AppxPackage *{app}* | Remove-AppxPackage -ErrorAction SilentlyContinue\"");
                    OperationProgress.Value = (i + 1) * progressStep;
                }

                OperationLog.Text += "\n\n✓ Bloatware removal completed successfully!";
            }
            catch (Exception ex)
            {
                OperationLog.Text += $"\n\n✗ Error: {ex.Message}";
            }
            finally
            {
                OperationProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ExecuteOneDriveRemovalAsync()
        {
            try
            {
                OperationLog.Text += "\nClosing OneDrive...";
                await RunCommandAsync("taskkill /f /im OneDrive.exe");
                await Task.Delay(1000);
                
                OperationLog.Text += "\nRunning uninstaller...";
                if (Environment.Is64BitOperatingSystem)
                    await RunCommandAsync(@"C:\Windows\SysWOW64\OneDriveSetup.exe /uninstall");
                else
                    await RunCommandAsync(@"C:\Windows\System32\OneDriveSetup.exe /uninstall");

                OperationLog.Text += "\n\n✓ OneDrive removal initiated!";
            }
            catch (Exception ex)
            {
                OperationLog.Text += $"\n\n✗ Error: {ex.Message}";
            }
        }

        private async Task ExecuteOptimizationAsync()
        {
            try
            {
                OperationLog.Text += "\n• Disabling telemetry services...";
                OperationProgress.Value = 20;
                await RunCommandAsync("sc config DiagTrack start=disabled");
                await RunCommandAsync("sc stop DiagTrack");

                OperationLog.Text += "\n• Disabling feedback notifications...";
                OperationProgress.Value = 40;
                await RunCommandAsync("sc config dmwappushservice start=disabled");

                OperationLog.Text += "\n• Disabling Consumer Features...";
                OperationProgress.Value = 60;
                await RunCommandAsync("reg add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager\" /v \"SystemPaneSuggestionsEnabled\" /t REG_DWORD /d 0 /f");

                OperationLog.Text += "\n• Cleaning temporary files...";
                OperationProgress.Value = 80;
                await RunCommandAsync("powershell -Command \"Remove-Item -Path $env:TEMP\\* -Recurse -Force -ErrorAction SilelyContinue\"");

                OperationProgress.Value = 100;
                OperationLog.Text += "\n\n✓ System optimization completed!";
            }
            catch (Exception ex)
            {
                OperationLog.Text += $"\n\n✗ Error: {ex.Message}";
            }
            finally
            {
                await Task.Delay(1000);
                OperationProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async Task RunCommandAsync(string command)
        {
            await Task.Run(() =>
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {command}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(processInfo))
                    {
                        process?.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Command execution error: {ex.Message}");
                }
            });
        }
    }
}
