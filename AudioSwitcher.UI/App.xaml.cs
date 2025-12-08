using Microsoft.UI.Xaml;

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
            Window.Activate();
        }

        public FlyoutWindow? Window { get; private set; }
    }
}
