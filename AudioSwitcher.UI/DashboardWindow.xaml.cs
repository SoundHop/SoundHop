using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using AudioSwitcher.Core.Interop;
using AudioSwitcher.UI.Services;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace AudioSwitcher.UI
{
    public sealed partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            this.InitializeComponent();
            this.Title = "Audio Switcher Dashboard";
            
            // Apply Mica backdrop
            this.SystemBackdrop = new MicaBackdrop();
            
            // Standard window setup
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var wndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(wndId);
            
            // Extend content into title bar for full Mica effect
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverBackgroundColor = Colors.Transparent;
                titleBar.ButtonPressedBackgroundColor = Colors.Transparent;
            }
            
            appWindow.Resize(new Windows.Graphics.SizeInt32(1000, 700));

            // Handle Closing
            appWindow.Closing += AppWindow_Closing;
            
            // Handle Minimize
            appWindow.Changed += AppWindow_Changed;
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            if (SettingsService.Instance.CloseToTray)
            {
                args.Cancel = true;
                sender.Hide();
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidPresenterChange)
            {
                if (sender.Presenter is OverlappedPresenter presenter)
                {
                    if (presenter.State == OverlappedPresenterState.Minimized)
                    {
                        if (SettingsService.Instance.MinimizeToTray)
                        {
                            sender.Hide();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Navigates to the Settings page within the dashboard.
        /// </summary>
        public void NavigateToSettings()
        {
            ShellPage.NavigateToSettings();
        }
    }
}
