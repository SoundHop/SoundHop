using Microsoft.UI.Xaml.Controls;
using AudioSwitcher.UI.ViewModels;
using AudioSwitcher.Core.Models;

namespace AudioSwitcher.UI.Views
{
    public sealed partial class FlyoutPage : Page
    {
        public MainViewModel ViewModel { get; }

        public FlyoutPage()
        {
            this.InitializeComponent();
            ViewModel = MainViewModel.Instance;
            ViewModel.Initialize();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            DeviceListControl.ViewModel = ViewModel;
            
            // Set initial tray icon from default device
            this.Loaded += (s, e) =>
            {
                if (ViewModel.DefaultDevice != null)
                {
                    UpdateTrayIcon(ViewModel.DefaultDevice);
                }
            };
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.DefaultDevice) && ViewModel.DefaultDevice != null)
            {
               UpdateTrayIcon(ViewModel.DefaultDevice);
            }
        }

        private void UpdateTrayIcon(AudioSwitcher.Core.Models.AudioDevice device)
        {
            string glyph = device.DisplayIcon;
            string tooltip = device.Name;
            
            System.Diagnostics.Trace.WriteLine($"FlyoutPage.UpdateTrayIcon: glyph='{glyph}', tooltip='{tooltip}'");
            
            if (Microsoft.UI.Xaml.Application.Current is App app && app.Window is FlyoutWindow flyoutWindow)
            {
                System.Diagnostics.Trace.WriteLine($"FlyoutPage.UpdateTrayIcon: TrayManager is {(flyoutWindow.TrayManager != null ? "not null" : "NULL")}");
                flyoutWindow.UpdateIconFromFluentGlyph(glyph, tooltip);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("FlyoutPage.UpdateTrayIcon: Could not access MainWindow");
            }
        }

        private void OpenDashboard_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (Microsoft.UI.Xaml.Application.Current is App app && app.Window is FlyoutWindow flyoutWindow)
            {
                flyoutWindow.OpenDashboard();
            }
        }
    }
}
