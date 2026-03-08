using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Fluent_Terminal
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);

            // Navigate to the dashboard by default
            ContentFrame.Navigate(typeof(DashboardPage));
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
                return;
            }

            if (args.InvokedItemContainer.Tag != null)
            {
                string tag = args.InvokedItemContainer.Tag.ToString() ?? "";
                switch (tag)
                {
                    case "dashboard":
                        ContentFrame.Navigate(typeof(DashboardPage));
                        break;
                    case "terminal":
                        ContentFrame.Navigate(typeof(TerminalPage));
                        break;
                    case "wsl":
                        ContentFrame.Navigate(typeof(WslPage));
                        break;
                    case "maintenance":
                        ContentFrame.Navigate(typeof(MaintenancePage));
                        break;
                    case "disk":
                        ContentFrame.Navigate(typeof(DiskAnalyzerPage));
                        break;
                    case "debloater":
                        ContentFrame.Navigate(typeof(DebloaterPage));
                        break;
                    case "startup":
                        ContentFrame.Navigate(typeof(StartupAppsPage));
                        break;
                    case "tweaks":
                        ContentFrame.Navigate(typeof(SystemTweaksPage));
                        break;
                }
            }
        }
    }
}