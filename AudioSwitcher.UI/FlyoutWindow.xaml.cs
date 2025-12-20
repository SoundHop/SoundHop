using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
using AudioSwitcher.UI.Helpers;
using AudioSwitcher.Core.Interop;
using AudioSwitcher.UI.SystemTray.Core;
using AudioSwitcher.UI.SystemTray.UI;
using AudioSwitcher.UI.Services;

namespace AudioSwitcher.UI
{
    /// <summary>
    /// Main flyout window for SoundHop.
    /// Positioning logic adapted from EarTrumpet (https://github.com/File-New-Project/EarTrumpet) - MIT License.
    /// </summary>
    public sealed partial class FlyoutWindow : Window
    {
        public SystemTrayManager TrayManager { get; }
        private readonly AppWindow _appWindow;
        private readonly IntPtr _hWnd;
        private WindowsTaskbar.State _lastTaskbarState;
        private bool _isAnimating;
        private DashboardWindow? _dashboardWindow;

        public FlyoutWindow()
        {
            this.InitializeComponent();

            this.Title = "SoundHop";

            // Get window handle and AppWindow
            _hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(_hWnd);
            _appWindow = AppWindow.GetFromWindowId(wndId);
            
            // Hide title bar for cleaner popup look
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
               _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
               _appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Collapsed;
               _appWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
               _appWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
            }

            // Force DWM Dark Mode to prevent white flash
            UpdateTheme(Application.Current.RequestedTheme == ApplicationTheme.Dark);

            // Initialize SystemTrayManager
            // We use a dummy window helper because we manage our own window visibility/positioning
            var windowHelper = new WindowHelper(this);
            TrayManager = new SystemTrayManager(windowHelper)
            {
                IconToolTip = "SoundHop",
                MinimizeToTray = false, // We control visibility manually
                CloseButtonMinimizesToTray = false,
                IsIconVisible = SettingsService.Instance.ShowTrayIcon
            };

            // Action to open dashboard (just open the window)
            TrayManager.OpenDashboardAction = () => 
            {
                OpenDashboard();
            };

            // Action to open settings (open dashboard and navigate to settings)
            TrayManager.OpenSettingsAction = () => 
            {
                var dashboard = OpenDashboard();
                dashboard.NavigateToSettings();
            };

            // Action to open hotkeys (open dashboard and navigate to hotkeys)
            TrayManager.OpenHotkeysAction = () => 
            {
                var dashboard = OpenDashboard();
                dashboard.NavigateToHotkeys();
            };
            
            // Listen for settings changes
            SettingsService.Instance.SettingChanged += (s, settingName) =>
            {
                if (settingName == nameof(SettingsService.ShowTrayIcon))
                {
                    TrayManager.IsIconVisible = SettingsService.Instance.ShowTrayIcon;
                }
            };

            // Toggle flyout on left click
            TrayManager.SystemTrayIcon.LeftClick += (s, e) =>
            {
                if (_isAnimating) return;

                if (this.Visible)
                {
                    HideWithAnimation();
                }
                else
                {
                    ShowWithAnimation();
                }
            };

            // Quick switch on middle click (cycle favorites)
            TrayManager.SystemTrayIcon.MiddleClick += (s, e) =>
            {
                if (SettingsService.Instance.QuickSwitchMode)
                {
                    ViewModels.MainViewModel.Instance.CycleToNextFavorite();
                }
            };

            // Auto-hide when focus lost
            this.Activated += MainWindow_Activated;

            // Set initial size
            _appWindow.Resize(new Windows.Graphics.SizeInt32(450, 590));
        }

        public void UpdateIconFromFluentGlyph(string glyph, string? tooltip = null)
        {
            if (tooltip != null)
            {
                TrayManager.IconToolTip = tooltip;
            }

            bool isDarkTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark;
            
            var hIcon = CreateIconFromFluentGlyph(glyph, isDarkTheme);
            if (hIcon != IntPtr.Zero)
            {
                // HIconHandle takes ownership and will destroy icon on dispose
                // SystemTrayIcon.Icon property disposes the old icon if needed.
                TrayManager.SystemTrayIcon.Icon = new HIconHandle(hIcon, true);
            }
        }

        private IntPtr CreateIconFromFluentGlyph(string glyph, bool isDarkTheme)
        {
            // Use GDI+ to draw the glyph to an icon
            // 32x32 is standard for tray icons
            using (var bitmap = new System.Drawing.Bitmap(32, 32))
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                
                // Clear background (transparent)
                g.Clear(System.Drawing.Color.Transparent);

                // Determine color based on theme
                var brush = System.Drawing.Brushes.White;

                // Create font - use 30px to fill more of the icon area
                // "Segoe Fluent Icons" might need to be installed or use fallback
                using (var font = new System.Drawing.Font("Segoe Fluent Icons", 30, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel))
                {
                    // Measure string to center it
                    var size = g.MeasureString(glyph, font);
                    float x = (32 - size.Width) / 2;
                    float y = (32 - size.Height) / 2;
                    
                    g.DrawString(glyph, font, brush, x, y);
                }
                
                return bitmap.GetHicon();
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            // Don't hide if we are animating (either showing or hiding)
            // This prevents the window from immediately hiding when taskbar is brought to front during show
            if (args.WindowActivationState == WindowActivationState.Deactivated && !_isAnimating)
            {
                HideWithAnimation();
            }
        }
        
        public void Hide()
        {
            if (!_isAnimating)
            {
                HideWithAnimation();
            }
        }

        private void ShowWithAnimation()
        {
            // Set animating flag BEFORE showing to prevent deactivation handler from hiding
            _isAnimating = true;
            
            // Re-apply DWM dark mode to prevent white flash after idle
            UpdateTheme(Application.Current.RequestedTheme == ApplicationTheme.Dark);
            
            // Get taskbar state for positioning (uses cursor position for multi-monitor support)
            _lastTaskbarState = WindowsTaskbar.GetStateFromCursor();
            
            // Position the window
            PositionFlyout(_lastTaskbarState);

            // === Layered window approach to prevent white flash ===
            // Make window layered and set alpha to 0 before showing
            int exStyle = GetWindowLong(_hWnd, GWL_EXSTYLE);
            SetWindowLong(_hWnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
            SetLayeredWindowAttributes(_hWnd, 0, 0, LWA_ALPHA); // Alpha = 0 (invisible)

            // Show window (invisible due to alpha=0)
            ShowWindow(_hWnd, 5); // SW_SHOW
            this.Activate();

            // Bring taskbar to front first (like EarTrumpet does)
            WindowsTaskbar.SetForegroundWindow(WindowsTaskbar.GetTaskbarHwnd());

            // Ensure content is laid out
            this.Content.UpdateLayout();

            // On next frame, set alpha to 255 and start animation
            // This gives the compositor time to initialize the backdrop
            DispatcherQueue.TryEnqueue(() =>
            {
                // Set alpha to 255 (fully visible) - backdrop should be ready now
                SetLayeredWindowAttributes(_hWnd, 0, 255, LWA_ALPHA);
                
                // Start entrance animation
                AudioSwitcher.UI.Helpers.FlyoutAnimationHelper.BeginEntranceAnimation(this.Content, _lastTaskbarState.Location, () =>
                {
                    _isAnimating = false;
                    AudioSwitcher.Core.Interop.User32.SetForegroundWindow(_hWnd);
                });
            });
        }

        private void HideWithAnimation()
        {
            _isAnimating = true;
            AudioSwitcher.UI.Helpers.FlyoutAnimationHelper.BeginExitAnimation(this.Content, _lastTaskbarState.Location, () =>
            {
                _isAnimating = false;
                ShowWindow(_hWnd, 0); // SW_HIDE
                AudioSwitcher.UI.Helpers.FlyoutAnimationHelper.ResetVisual(this.Content);
            });
        }

        private void PositionFlyout(AudioSwitcher.Core.Interop.WindowsTaskbar.State taskbar)
        {
            int width = _appWindow.Size.Width;
            int height = _appWindow.Size.Height;
            
            // Get base work area
            var workArea = taskbar.WorkArea;
            
            // Adjust work area for auto-hide taskbar (EarTrumpet approach)
            int adjustedLeft = workArea.Left;
            int adjustedRight = workArea.Right;
            int adjustedTop = workArea.Top;
            int adjustedBottom = workArea.Bottom;

            if (taskbar.IsAutoHideEnabled)
            {
                var tb = taskbar.TaskbarBounds;
                switch (taskbar.Location)
                {
                    case AudioSwitcher.Core.Interop.WindowsTaskbar.Position.Left:
                        if (workArea.Left < tb.Right) adjustedLeft = tb.Right;
                        break;
                    case AudioSwitcher.Core.Interop.WindowsTaskbar.Position.Right:
                        if (workArea.Right > tb.Left) adjustedRight = tb.Left;
                        break;
                    case AudioSwitcher.Core.Interop.WindowsTaskbar.Position.Top:
                        if (workArea.Top < tb.Bottom) adjustedTop = tb.Bottom;
                        break;
                    case AudioSwitcher.Core.Interop.WindowsTaskbar.Position.Bottom:
                        if (workArea.Bottom > tb.Top) adjustedBottom = tb.Top;
                        break;
                }
            }
            
            // Get cursor position for horizontal centering
            AudioSwitcher.Core.Interop.User32.GetCursorPos(out var cursorPos);

            int x, y;
            int padding = 12; // Gap between flyout and taskbar/screen edge

            // Position based on taskbar location
            switch (taskbar.Location)
            {
                case AudioSwitcher.Core.Interop.WindowsTaskbar.Position.Top:
                    x = cursorPos.X - (width / 2);
                    y = adjustedTop + padding;
                    break;
                case AudioSwitcher.Core.Interop.WindowsTaskbar.Position.Left:
                    x = adjustedLeft + padding;
                    y = cursorPos.Y - (height / 2);
                    break;
                case AudioSwitcher.Core.Interop.WindowsTaskbar.Position.Right:
                    x = adjustedRight - width - padding;
                    y = cursorPos.Y - (height / 2);
                    break;
                case AudioSwitcher.Core.Interop.WindowsTaskbar.Position.Bottom:
                default:
                    x = cursorPos.X - (width / 2);
                    y = adjustedBottom - height - padding;
                    break;
            }

            // Clamp to adjusted work area bounds
            if (x + width > adjustedRight) x = adjustedRight - width - padding;
            if (x < adjustedLeft) x = adjustedLeft + padding;
            if (y + height > adjustedBottom) y = adjustedBottom - height - padding;
            if (y < adjustedTop) y = adjustedTop + padding;

            System.Diagnostics.Trace.WriteLine($"MainWindow.PositionFlyout: x={x}, y={y}, adjusted={adjustedLeft},{adjustedTop},{adjustedRight},{adjustedBottom}, autoHide={taskbar.IsAutoHideEnabled}");
            _appWindow.Move(new Windows.Graphics.PointInt32(x, y));
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // Layered window support for alpha blending to prevent white flash
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const uint LWA_ALPHA = 0x02;

        private unsafe void UpdateTheme(bool isDarkTheme)
        {
            int isDark = isDarkTheme ? 1 : 0;
            DwmSetWindowAttribute(_hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE, new IntPtr(&isDark), sizeof(int));
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, IntPtr pvAttribute, int cbAttribute);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        /// <summary>
        /// Opens the dashboard window, creating it if necessary, and returns it.
        /// </summary>
        public DashboardWindow OpenDashboard()
        {
            if (_dashboardWindow == null)
            {
                _dashboardWindow = new DashboardWindow();
                _dashboardWindow.Closed += (s, e) => _dashboardWindow = null;
            }
            _dashboardWindow.Activate();
            return _dashboardWindow;
        }
    }
}
