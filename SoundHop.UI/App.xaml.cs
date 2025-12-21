using Microsoft.UI.Xaml;
using SoundHop.UI.Services;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

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
            
            // Check for updates in the background
            _ = CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            // Check if auto-updates are enabled
            if (!SoundHop.Core.Services.SettingsService.Instance.Settings.AutoCheckUpdates)
                return;
            
            try
            {
                var mgr = new UpdateManager(new GithubSource("https://github.com/SoundHop/SoundHop", null, false));
                
                // Check for new version
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                    return; // No update available
                
                // Download updates in background
                await mgr.DownloadUpdatesAsync(newVersion);
                
                // Store the pending update info for later (user can apply from Settings)
                PendingUpdate = newVersion;
            }
            catch
            {
                // Silently ignore update check failures
            }
        }
        
        /// <summary>
        /// Pending update info, if an update was downloaded.
        /// </summary>
        public static UpdateInfo? PendingUpdate { get; private set; }
        
        /// <summary>
        /// Applies the pending update and restarts the app.
        /// </summary>
        public static void ApplyPendingUpdate()
        {
            if (PendingUpdate == null) return;
            
            var mgr = new UpdateManager(new GithubSource("https://github.com/SoundHop/SoundHop", null, false));
            mgr.ApplyUpdatesAndRestart(PendingUpdate);
        }

        public FlyoutWindow? Window { get; private set; }
    }
}
