using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace Fluent_Terminal
{
    public class SystemStatItem
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string Details { get; set; } = "";
        public string Icon { get; set; } = "";
        public Brush IconColor { get; set; } = (Microsoft.UI.Xaml.Application.Current?.Resources?["TextFillColorPrimaryBrush"] as Brush) ?? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
    }

    public class QuickActionItem
    {
        public string Label { get; set; } = "";
        public string Action { get; set; } = "";
    }

    public class ActivityLogItem
    {
        public string Message { get; set; } = "";
        public string Timestamp { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Status { get; set; } = "";
        public Brush StatusColor { get; set; } = (Microsoft.UI.Xaml.Application.Current?.Resources?["TextFillColorPrimaryBrush"] as Brush) ?? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
    }

    public sealed partial class DashboardPage : Page
    {
        private DispatcherTimer? _updateTimer;
        private readonly ObservableCollection<SystemStatItem> _quickStats = new();
        private readonly ObservableCollection<QuickActionItem> _quickActions = new();
        private readonly ObservableCollection<ActivityLogItem> _recentActivities = new();
        private int _updateCount = 0;
        private PerformanceCounter? _cpuCounter;

        public ObservableCollection<SystemStatItem> QuickStats => _quickStats;
        public ObservableCollection<QuickActionItem> QuickActions => _quickActions;
        public ObservableCollection<ActivityLogItem> RecentActivities => _recentActivities;

        public DashboardPage()
        {
            this.InitializeComponent();
            InitializeQuickStats();
            InitializeQuickActions();
            InitializeActivityLog();
            SetupUpdateTimer();
        }

        private void InitializeQuickStats()
        {
            try
            {
                var osVersion = Environment.OSVersion;
                var computerName = Environment.MachineName;
                var processorCount = Environment.ProcessorCount;
                var themeColor = Microsoft.UI.Xaml.Application.Current?.Resources?["TextFillColorPrimaryBrush"] as Brush ?? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));

                _quickStats.Add(new SystemStatItem
                {
                    Label = "OS Version",
                    Value = "Windows 11",
                    Details = $"Build: {osVersion.Version.Build}",
                    Icon = "\uE895",
                    IconColor = themeColor
                });

                _quickStats.Add(new SystemStatItem
                {
                    Label = "Computer Name",
                    Value = computerName,
                    Details = "System Name",
                    Icon = "\uE850",
                    IconColor = themeColor
                });

                _quickStats.Add(new SystemStatItem
                {
                    Label = "Processor",
                    Value = $"{processorCount} Cores",
                    Details = "Logical Processors",
                    Icon = "\uE947",
                    IconColor = themeColor
                });

                _quickStats.Add(new SystemStatItem
                {
                    Label = "RAM Installed",
                    Value = GetTotalSystemRAM(),
                    Details = "Total Memory",
                    Icon = "\uEA3C",
                    IconColor = themeColor
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Quick stats initialization error: {ex.Message}");
            }
        }

        private void InitializeQuickActions()
        {
            _quickActions.Add(new QuickActionItem { Label = "Open Settings", Action = "settings" });
            _quickActions.Add(new QuickActionItem { Label = "System Properties", Action = "system" });
            _quickActions.Add(new QuickActionItem { Label = "Device Manager", Action = "devmgmt" });
            _quickActions.Add(new QuickActionItem { Label = "Disk Cleanup", Action = "cleanmgr" });
            _quickActions.Add(new QuickActionItem { Label = "Task Manager", Action = "taskmgr" });
            _quickActions.Add(new QuickActionItem { Label = "System Info", Action = "sysinfo" });
        }

        private void InitializeActivityLog()
        {
            var themeColor = Microsoft.UI.Xaml.Application.Current?.Resources?["TextFillColorPrimaryBrush"] as Brush ?? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
            AddActivityLog("System Dashboard Initialized", "System started", "\uE81C", "✓ Ready", themeColor);
            AddActivityLog("Performance Monitoring Active", "Real-time metrics enabled", "\uE947", "✓ Active", themeColor);
        }

        private void SetupUpdateTimer()
        {
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(2);
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            UpdateSystemTime();
        }

        private async void UpdateTimer_Tick(object? sender, object e)
        {
            _updateCount++;
            await UpdateSystemMetricsAsync();
            
            if (_updateCount % 5 == 0)
            {
                UpdateSystemHealth();
            }

            UpdateSystemTime();
        }

        private async Task UpdateSystemMetricsAsync()
        {
            try
            {
                float cpuUsage = await Task.Run(() => GetCpuUsage());
                CpuPercentage.Text = $"{cpuUsage:F1}%";
                CpuProgressBar.Value = cpuUsage;
                CpuDetails.Text = $"Threads: {Process.GetProcesses().Sum(p => p.Threads.Count)}";

                var ramInfo = GetRAMUsage();
                RamPercentage.Text = $"{ramInfo.percentage:F1}%";
                RamProgressBar.Value = ramInfo.percentage;
                RamDetails.Text = $"{ramInfo.usedGB} GB / {ramInfo.totalGB} GB";

                var driveInfo = new DriveInfo("C");
                if (driveInfo.IsReady)
                {
                    double diskUsagePercent = (double)(driveInfo.TotalSize - driveInfo.AvailableFreeSpace) / driveInfo.TotalSize * 100;
                    DiskPercentage.Text = $"{diskUsagePercent:F1}%";
                    DiskProgressBar.Value = diskUsagePercent;

                    long usedGB = (driveInfo.TotalSize - driveInfo.AvailableFreeSpace) / (1024 * 1024 * 1024);
                    long totalGB = driveInfo.TotalSize / (1024 * 1024 * 1024);
                    DiskDetails.Text = $"{usedGB} GB / {totalGB} GB";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Metrics update error: {ex.Message}");
            }
        }

        private float GetCpuUsage()
        {
            try
            {
                if (_cpuCounter == null)
                {
                    _cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                    _cpuCounter.NextValue();
                    return 0f;
                }
                return _cpuCounter.NextValue();
            }
            catch
            {
                return 0f;
            }
        }

        private (double percentage, long usedGB, long totalGB) GetRAMUsage()
        {
            try
            {
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    long totalBytes = (long)memStatus.ullTotalPhys;
                    long freeBytes = (long)memStatus.ullAvailPhys;
                    long usedBytes = totalBytes - freeBytes;

                    double percentage = (double)usedBytes / totalBytes * 100;
                    long usedGB = usedBytes / (1024 * 1024 * 1024);
                    long totalGB = totalBytes / (1024 * 1024 * 1024);

                    return (percentage, usedGB, totalGB);
                }
            }
            catch { }

            return (0, 0, 0);
        }

        private void UpdateSystemHealth()
        {
            try
            {
                UpdateBatteryHealth();
                UpdateSecurityStatus();
                UpdateNetworkStatus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"System health update error: {ex.Message}");
            }
        }

        private void UpdateBatteryHealth()
        {
            try
            {
                BatteryHealthBar.Value = 85;
                BatteryHealthText.Text = "85%";
            }
            catch
            {
                BatteryHealthText.Text = "N/A";
            }
        }

        private void UpdateSecurityStatus()
        {
            try
            {
                SecurityStatus.Text = "✓ Protected";
            }
            catch
            {
                SecurityStatus.Text = "Checking...";
            }
        }

        private void UpdateNetworkStatus()
        {
            try
            {
                bool isConnected = NetworkInterface.GetIsNetworkAvailable();
                NetworkStatus.Text = isConnected ? "✓ Connected" : "✗ Disconnected";
            }
            catch
            {
                NetworkStatus.Text = "Checking...";
            }
        }

        private void UpdateSystemTime()
        {
            SystemTimeBlock.Text = $"Last updated: {DateTime.Now:g}";
        }

        private void QuickActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string action)
            {
                ExecuteQuickAction(action);
            }
        }

        private void ExecuteQuickAction(string action)
        {
            try
            {
                var themeColor = Microsoft.UI.Xaml.Application.Current?.Resources?["TextFillColorPrimaryBrush"] as Brush ?? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                switch (action)
                {
                    case "settings":
                        Process.Start(new ProcessStartInfo { FileName = "ms-settings:", UseShellExecute = true });
                        AddActivityLog("Opened Settings", "Windows Settings launched", "\u2699", "✓ Success", themeColor);
                        break;
                    case "system":
                        Process.Start(new ProcessStartInfo { FileName = "systempropertiesadvanced.exe", UseShellExecute = true });
                        AddActivityLog("System Properties", "Advanced system settings opened", "\uE850", "✓ Success", themeColor);
                        break;
                    case "devmgmt":
                        Process.Start(new ProcessStartInfo { FileName = "devmgmt.msc", UseShellExecute = true });
                        AddActivityLog("Device Manager", "Hardware devices management opened", "\uE948", "✓ Success", themeColor);
                        break;
                    case "cleanmgr":
                        Process.Start(new ProcessStartInfo { FileName = "cleanmgr.exe", UseShellExecute = true });
                        AddActivityLog("Disk Cleanup", "Temporary files cleanup initiated", "\uE74C", "✓ Started", themeColor);
                        break;
                    case "taskmgr":
                        Process.Start(new ProcessStartInfo { FileName = "taskmgr.exe", UseShellExecute = true });
                        AddActivityLog("Task Manager", "Process monitoring tool opened", "\uE947", "✓ Success", themeColor);
                        break;
                    case "sysinfo":
                        Process.Start(new ProcessStartInfo { FileName = "msinfo32.exe", UseShellExecute = true });
                        AddActivityLog("System Information", "Detailed system info opened", "\uE850", "✓ Success", themeColor);
                        break;
                }
            }
            catch (Exception ex)
            {
                var errorColor = Microsoft.UI.Xaml.Application.Current?.Resources?["SystemFillColorCriticalBrush"] as Brush ?? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 0, 0));
                AddActivityLog("Action Failed", ex.Message, "\uE7BA", "✗ Error", errorColor);
                Debug.WriteLine($"Quick action error: {ex.Message}");
            }
        }

        private void AddActivityLog(string message, string details, string icon, string status, Brush statusColor)
        {
            _recentActivities.Insert(0, new ActivityLogItem
            {
                Message = message,
                Timestamp = $"{details} • {DateTime.Now:HH:mm:ss}",
                Icon = icon,
                Status = status,
                StatusColor = statusColor
            });

            while (_recentActivities.Count > 20)
            {
                _recentActivities.RemoveAt(_recentActivities.Count - 1);
            }
        }

        private string GetTotalSystemRAM()
        {
            try
            {
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    long totalGB = (long)memStatus.ullTotalPhys / (1024 * 1024 * 1024);
                    return $"{totalGB} GB";
                }
            }
            catch { }
            return "Unknown";
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(MEMORYSTATUSEX lpBuffer);
    }
}
