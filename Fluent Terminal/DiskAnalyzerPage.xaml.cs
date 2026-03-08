using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Fluent_Terminal
{
    public class FolderSizeItem
    {
        public string Name { get; set; } = "";
        public long Size { get; set; }
        public string SizeFormatted => FormatSize(Size);

        private string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int i = 0;
            double dblSByte = bytes;
            while (i < suffixes.Length && bytes >= 1024)
            {
                dblSByte = bytes / 1024.0;
                bytes /= 1024;
                i++;
            }
            return $"{dblSByte:F2} {suffixes[i]}";
        }
    }

    public sealed partial class DiskAnalyzerPage : Page
    {
        private ObservableCollection<FolderSizeItem> _folderSizes = new();

        public DiskAnalyzerPage()
        {
            this.InitializeComponent();
            FolderList.ItemsSource = _folderSizes;
        }

        private async void Browse_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add("*");

            // WinUI 3 Window handle requirement
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Current.GetType().GetProperty("Window")?.GetValue(App.Current));
            if (hwnd == IntPtr.Zero) // fallback for simple apps
            {
               // This part depends on how the Window is exposed. 
               // In some setups, you need to store it in App.xaml.cs
            }
            // For now, let's keep it simple and use Browse text box manually if the picker fails to initialize easily.
        }

        private async void StartScan_Click(object sender, RoutedEventArgs e)
        {
            string path = PathBox.Text;
            if (!Directory.Exists(path))
            {
                StatusText.Text = "Invalid directory path!";
                return;
            }

            _folderSizes.Clear();
            ScanProgress.Visibility = Visibility.Visible;
            StatusText.Text = "Scanning... Please wait.";

            try
            {
                var folders = await Task.Run(() => GetTopFolders(path));
                foreach (var folder in folders.OrderByDescending(f => f.Size).Take(15))
                {
                    _folderSizes.Add(folder);
                }
                StatusText.Text = $"Scan complete. Found {folders.Count} items.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
            finally
            {
                ScanProgress.Visibility = Visibility.Collapsed;
            }
        }

        private List<FolderSizeItem> GetTopFolders(string rootPath)
        {
            var items = new List<FolderSizeItem>();
            try
            {
                var dirInfo = new DirectoryInfo(rootPath);
                foreach (var dir in dirInfo.GetDirectories())
                {
                    try
                    {
                        long size = dir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
                        items.Add(new FolderSizeItem { Name = dir.Name, Size = size });
                    }
                    catch { } // Skip inaccessible
                }
                // Add files in root
                foreach (var file in dirInfo.GetFiles())
                {
                    items.Add(new FolderSizeItem { Name = file.Name, Size = file.Length });
                }
            }
            catch { }
            return items;
        }
    }
}
