using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using AudioSwitcher.Core.Interop;
using AudioSwitcher.UI.Services;

namespace AudioSwitcher.UI
{
    public sealed partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            this.InitializeComponent();
            this.Title = "Audio Switcher Dashboard";
            
            // Standard window setup
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var wndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(wndId);
            
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
               // Standard title bar for now
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
    }
}
