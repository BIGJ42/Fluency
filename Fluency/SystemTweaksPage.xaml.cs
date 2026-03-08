using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fluency
{
    public sealed partial class SystemTweaksPage : Page
    {
        public SystemTweaksPage()
        {
            this.InitializeComponent();
        }

        private async void DisableVisualEffects_Click(object sender, RoutedEventArgs e)
        {
            await ApplyTweak(@"
$regPath = 'HKCU:\Control Panel\Desktop'
Set-ItemProperty -Path $regPath -Name 'UserPreferencesMask' -Value ([byte[]](0x90, 0x12, 0x03, 0x80, 0x10, 0x00, 0x00, 0x00)) -ErrorAction SilentlyContinue
Write-Host 'Visual effects disabled'
", "Disable Visual Effects");
        }

        private async void DisableAnimations_Click(object sender, RoutedEventArgs e)
        {
            await ApplyTweak(@"
$regPath = 'HKCU:\Control Panel\Desktop'
$transitionPath = 'HKCU:\Control Panel\Desktop\WindowMetrics'
New-Item -Path $transitionPath -Force | Out-Null
Set-ItemProperty -Path $transitionPath -Name 'MinAnimate' -Value '0' -ErrorAction SilentlyContinue
Write-Host 'Animations disabled'
", "Disable Animations");
        }

        private async void DisableTransparency_Click(object sender, RoutedEventArgs e)
        {
            await ApplyTweak(@"
$regPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize'
if (-not (Test-Path $regPath)) { New-Item -Path $regPath -Force | Out-Null }
Set-ItemProperty -Path $regPath -Name 'EnableTransparency' -Value 0 -ErrorAction SilentlyContinue
Write-Host 'Transparency effects disabled'
", "Disable Transparency");
        }

        private async void DisableActivityHistory_Click(object sender, RoutedEventArgs e)
        {
            await ApplyTweak(@"
$regPath = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\System'
if (-not (Test-Path $regPath)) { New-Item -Path $regPath -Force | Out-Null }
Set-ItemProperty -Path $regPath -Name 'PublishUserActivities' -Value 0 -ErrorAction SilentlyContinue
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\SettingSync' -Name 'SyncSettings' -Value 0 -ErrorAction SilentlyContinue
Write-Host 'Activity history disabled'
", "Disable Activity History");
        }

        private async void DisableGameBar_Click(object sender, RoutedEventArgs e)
        {
            await ApplyTweak(@"
$regPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\GameDVR'
if (-not (Test-Path $regPath)) { New-Item -Path $regPath -Force | Out-Null }
Set-ItemProperty -Path $regPath -Name 'AppCaptureEnabled' -Value 0 -ErrorAction SilentlyContinue
Set-ItemProperty -Path $regPath -Name 'HistoricalCaptureDisabled' -Value 1 -ErrorAction SilentlyContinue
Write-Host 'Game Bar disabled'
", "Disable Game Bar");
        }

        private async void DisableCloudSync_Click(object sender, RoutedEventArgs e)
        {
            await ApplyTweak(@"
$regPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\SettingSync'
Set-ItemProperty -Path $regPath -Name 'SyncSettings' -Value 0 -ErrorAction SilentlyContinue
Set-ItemProperty -Path $regPath -Name 'SyncFlags' -Value 0 -ErrorAction SilentlyContinue
Write-Host 'Cloud sync disabled'
", "Disable Cloud Sync");
        }

        private async void RestoreContextMenu_Click(object sender, RoutedEventArgs e)
        {
            await ApplyTweak(@"
$regPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced'
New-Item -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'ContextMenus' -Force | Out-Null
Write-Host 'Context menu restored'
", "Restore Context Menu");
        }

        private async Task ApplyTweak(string script, string tweakName)
        {
            try
            {
                var result = await new ContentDialog
                {
                    Title = tweakName,
                    Content = $"Apply this tweak: {tweakName}?\n\nSome tweaks may require a restart.",
                    PrimaryButtonText = "Apply",
                    SecondaryButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await Task.Run(() =>
                    {
                        var psi = new ProcessStartInfo("powershell.exe", $"-NoProfile -Command \"{script.Replace("\"", "\\\"")}\"")
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        try
                        {
                            using (var process = Process.Start(psi))
                            {
                                if (process != null)
                                {
                                    process.WaitForExit();
                                }
                            }
                        }
                        catch { }
                    });

                    await new ContentDialog
                    {
                        Title = "Success",
                        Content = $"{tweakName} applied successfully!",
                        PrimaryButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    }.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to apply tweak: {ex.Message}",
                    PrimaryButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
        }
    }
}
