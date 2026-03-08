using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;

namespace Fluency
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            InitializeVersionText();
            SetInitialThemeSelection();
        }

        private void InitializeVersionText()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                if (version != null)
                {
                    VersionText.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
                }
            }
            catch { }
        }

        private void SetInitialThemeSelection()
        {
            // By default, just set to System Default for now
            ThemeComboBox.SelectedIndex = 0;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string themeTag)
            {
                // Note: WinUI 3 doesn't automatically propagate theme change to all windows via Application.Current.RequestedTheme
                // normally you would set it at the Window or Page level.
                // This is a simple implementation:
                switch (themeTag)
                {
                    case "Light":
                        RequestedTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        RequestedTheme = ElementTheme.Dark;
                        break;
                    default:
                        RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }

        private void TransparencyToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // Normally this would affect the MicaBackdrop.
            // Since we're in a page, we'd need to access the parent Window.
        }
    }
}
