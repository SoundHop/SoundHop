using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
using AudioSwitcher.UI.Services;
using AudioSwitcher.Core.Interop;

namespace AudioSwitcher.UI
{
    public sealed partial class MainWindow : Window
    {
        public ShellNotifyIcon TrayIcon { get; }
        private readonly AppWindow _appWindow;

        public MainWindow()
        {
            this.InitializeComponent();
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            this.Title = "Audio Switcher";

            // Get AppWindow for window manipulation
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(wndId);
            
            // Hide title bar for cleaner popup look
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
               _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
               _appWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
               _appWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
            }

            // Tray Icon (refactored ShellNotifyIcon)
            TrayIcon = new ShellNotifyIcon();
            TrayIcon.SetTooltip("Audio Switcher");
            TrayIcon.PrimaryInvoke += OnTrayPrimaryInvoke;
            TrayIcon.SecondaryInvoke += OnTraySecondaryInvoke;
            TrayIcon.TertiaryInvoke += OnTrayTertiaryInvoke;
            TrayIcon.Show();

            // Auto-hide when focus lost
            this.Activated += MainWindow_Activated;

            // Set initial size
            _appWindow.Resize(new Windows.Graphics.SizeInt32(450, 500));
        }

        private void OnTrayPrimaryInvoke(object? sender, EventArgs e)
        {
            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                ShowAtCursor();
            }
        }

        private void OnTraySecondaryInvoke(object? sender, ShellNotifyIcon.SecondaryInvokeArgs e)
        {
            // Right-click context menu - can be implemented later
            // e.Point contains the screen coordinates for positioning
        }

        private void OnTrayTertiaryInvoke(object? sender, EventArgs e)
        {
            // Middle-click - can be used for quick actions
        }

        private void ShowAtCursor()
        {
            User32.GetCursorPos(out var point);
            
            // Get DisplayArea relative to cursor
            var displayArea = DisplayArea.GetFromPoint(new Windows.Graphics.PointInt32(point.X, point.Y), DisplayAreaFallback.Primary);
            var workArea = displayArea.WorkArea;
            
            int width = _appWindow.Size.Width;
            int height = _appWindow.Size.Height;
            
            // Calculate Position (bottom-right aligned usually, but robust logic: center above icon?)
            // Simple logic: place center-x aligned with cursor, bottom aligned with workarea bottom - padding
            
            int x = point.X - (width / 2);
            int y = workArea.Height + workArea.Y - height - 12; // 12px padding from bottom

            // Clamp to screen
            if (x + width > workArea.X + workArea.Width) x = workArea.X + workArea.Width - width - 12;
            if (x < workArea.X) x = workArea.X + 12;

            _appWindow.Move(new Windows.Graphics.PointInt32(x, y));
            
            // Show and Activate
            this.Content.UpdateLayout(); // layout pass
            ShowWindow(WindowNative.GetWindowHandle(this), 5); // SW_SHOW
            User32.SetForegroundWindow(WindowNative.GetWindowHandle(this));
            this.Activate(); // WinUI activate
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                //this.Hide();
            }
        }
        
        public void Hide()
        {
           ShowWindow(WindowNative.GetWindowHandle(this), 0); // SW_HIDE
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
