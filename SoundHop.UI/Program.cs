using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Velopack;
using WinRT;

namespace SoundHop.UI
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Velopack must be the first thing to run
            VelopackApp.Build().Run();
            
            // Initialize WinUI
            ComWrappersSupport.InitializeComWrappers();
            Application.Start((p) =>
            {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }
    }
}
