using Microsoft.UI.Xaml;
using SoundHop.UI.Services;

namespace SoundHop.UI
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Window = new FlyoutWindow();
            
            if (SettingsService.Instance.StartMinimized)
            {
                // Activate briefly then hide - needed for tray icon to work
                Window.Activate();
                Window.Hide();
            }
            else
            {
                // Open Dashboard first, then quietly initialize the flyout in background
                var dashboard = Window.OpenDashboard();
                
                // Activate flyout briefly for tray icon, but it stays behind the dashboard
                Window.Activate();
                Window.Hide();
                
                // Bring dashboard to front
                dashboard.Activate();
            }
        }

        public FlyoutWindow? Window { get; private set; }
    }
}
