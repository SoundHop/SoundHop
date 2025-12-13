using Microsoft.UI.Xaml;
using AudioSwitcher.UI.Services;

namespace AudioSwitcher.UI
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
                Window.Activate();
            }
        }

        public FlyoutWindow? Window { get; private set; }
    }
}
