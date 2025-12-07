using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
using AudioSwitcher.UI.Services;
using AudioSwitcher.UI.Helpers;
using AudioSwitcher.Core.Interop;

namespace AudioSwitcher.UI
{
    /// <summary>
    /// Main flyout window for Audio Switcher.
    /// Positioning logic adapted from EarTrumpet (https://github.com/File-New-Project/EarTrumpet) - MIT License.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ShellNotifyIcon TrayIcon { get; }
        private readonly AppWindow _appWindow;
        private readonly IntPtr _hWnd;
        private WindowsTaskbar.State _lastTaskbarState;
        private bool _isAnimating;

        public MainWindow()
        {
            this.InitializeComponent();
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
            this.Title = "Audio Switcher";

            // Get window handle and AppWindow
            _hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(_hWnd);
            _appWindow = AppWindow.GetFromWindowId(wndId);
            
            // Hide title bar for cleaner popup look
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
               _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
               _appWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
               _appWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
            }

            // Tray Icon
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
            if (_isAnimating) return;

            if (this.Visible)
            {
                HideWithAnimation();
            }
            else
            {
                ShowWithAnimation();
            }
        }

        private void OnTraySecondaryInvoke(object? sender, ShellNotifyIcon.SecondaryInvokeArgs e)
        {
            // Pass the screen coordinates from the tray icon
            ShowContextMenu(e.Point);
        }

        private void ShowContextMenu(System.Drawing.Point trayIconPoint)
        {
            // Hide the flyout first if it's visible
            if (this.Visible && !_isAnimating)
            {
                ShowWindow(_hWnd, 0); // SW_HIDE immediately, no animation
            }

            // Get cursor position (more reliable than wParam coordinates)
            User32.GetCursorPos(out var cursorPos);
            
            // Position window at the cursor/tray icon location
            // Make the window tiny so the menu appears at the right spot
            _appWindow.Resize(new Windows.Graphics.SizeInt32(1, 1));
            _appWindow.Move(new Windows.Graphics.PointInt32(cursorPos.X, cursorPos.Y));
            ShowWindow(_hWnd, 5); // SW_SHOW
            
            // Bring window to front for menu
            User32.SetForegroundWindow(_hWnd);

            // Show the context menu - it will appear above/below the tiny window at cursor
            TrayContextMenu.Closed += OnContextMenuClosed;
            TrayContextMenu.ShowAt(RootGrid, new Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions
            {
                Position = new Windows.Foundation.Point(0, 0),
                Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.TopEdgeAlignedLeft
            });
        }

        private void OnContextMenuClosed(object? sender, object e)
        {
            TrayContextMenu.Closed -= OnContextMenuClosed;
            
            // Hide the helper window and restore size
            ShowWindow(_hWnd, 0); // SW_HIDE
            _appWindow.Resize(new Windows.Graphics.SizeInt32(450, 500));
        }

        private void OnSettingsClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // TODO: Open settings window/page
            System.Diagnostics.Trace.WriteLine("Settings clicked");
        }

        private void OnExitClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Clean up and exit
            TrayIcon.Dispose();
            Application.Current.Exit();
        }

        private void OnTrayTertiaryInvoke(object? sender, EventArgs e)
        {
            // Middle-click - can be used for quick actions
        }

        private void ShowWithAnimation()
        {
            // Get taskbar state for positioning
            _lastTaskbarState = WindowsTaskbar.Current;
            
            // Position the window
            PositionFlyout(_lastTaskbarState);

            // Bring taskbar to front first (like EarTrumpet does)
            WindowsTaskbar.SetForegroundWindow(WindowsTaskbar.GetTaskbarHwnd());

            // Show window
            ShowWindow(_hWnd, 5); // SW_SHOW
            this.Activate();

            // Ensure content is laid out before animating
            this.Content.UpdateLayout();

            // Play entrance animation on next frame to ensure visual is ready
            _isAnimating = true;
            DispatcherQueue.TryEnqueue(() =>
            {
                FlyoutAnimationHelper.BeginEntranceAnimation(this.Content, _lastTaskbarState.Location, () =>
                {
                    _isAnimating = false;
                    // Set as topmost and focus after animation
                    User32.SetForegroundWindow(_hWnd);
                });
            });
        }

        private void HideWithAnimation()
        {
            _isAnimating = true;
            FlyoutAnimationHelper.BeginExitAnimation(this.Content, _lastTaskbarState.Location, () =>
            {
                _isAnimating = false;
                ShowWindow(_hWnd, 0); // SW_HIDE
                FlyoutAnimationHelper.ResetVisual(this.Content);
            });
        }

        private void PositionFlyout(WindowsTaskbar.State taskbar)
        {
            int width = _appWindow.Size.Width;
            int height = _appWindow.Size.Height;
            
            // Get base work area
            var workArea = taskbar.WorkArea;
            
            // Adjust work area for auto-hide taskbar (EarTrumpet approach)
            // When auto-hide is enabled, the working area doesn't account for the taskbar
            // So we manually carve out space for it
            int adjustedLeft = workArea.Left;
            int adjustedRight = workArea.Right;
            int adjustedTop = workArea.Top;
            int adjustedBottom = workArea.Bottom;

            if (taskbar.IsAutoHideEnabled)
            {
                var tb = taskbar.TaskbarBounds;
                switch (taskbar.Location)
                {
                    case WindowsTaskbar.Position.Left:
                        if (workArea.Left < tb.Right)
                            adjustedLeft = tb.Right;
                        break;
                    case WindowsTaskbar.Position.Right:
                        if (workArea.Right > tb.Left)
                            adjustedRight = tb.Left;
                        break;
                    case WindowsTaskbar.Position.Top:
                        if (workArea.Top < tb.Bottom)
                            adjustedTop = tb.Bottom;
                        break;
                    case WindowsTaskbar.Position.Bottom:
                        if (workArea.Bottom > tb.Top)
                            adjustedBottom = tb.Top;
                        break;
                }
            }
            
            // Get cursor position for horizontal centering
            User32.GetCursorPos(out var cursorPos);

            int x, y;
            int padding = 12; // Gap between flyout and taskbar/screen edge

            // Position based on taskbar location
            switch (taskbar.Location)
            {
                case WindowsTaskbar.Position.Top:
                    x = cursorPos.X - (width / 2);
                    y = adjustedTop + padding;
                    break;
                    
                case WindowsTaskbar.Position.Left:
                    x = adjustedLeft + padding;
                    y = cursorPos.Y - (height / 2);
                    break;
                    
                case WindowsTaskbar.Position.Right:
                    x = adjustedRight - width - padding;
                    y = cursorPos.Y - (height / 2);
                    break;
                    
                case WindowsTaskbar.Position.Bottom:
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

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
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

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
