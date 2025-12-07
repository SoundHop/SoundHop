using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using AudioSwitcher.Core.Interop;
using Microsoft.UI.Dispatching;

namespace AudioSwitcher.UI.Services
{
    /// <summary>
    /// Manages a system tray (notification area) icon using Win32 Shell_NotifyIcon.
    /// Refactored based on EarTrumpet's ShellNotifyIcon pattern.
    /// </summary>
    public class ShellNotifyIcon : IDisposable
    {
        /// <summary>
        /// Arguments for secondary (right-click) invoke events, including screen coordinates.
        /// </summary>
        public class SecondaryInvokeArgs
        {
            public Point Point { get; set; }
        }

        // Events for different click types
        public event EventHandler? PrimaryInvoke;      // Left click
        public event EventHandler<SecondaryInvokeArgs>? SecondaryInvoke;  // Right click
        public event EventHandler? TertiaryInvoke;     // Middle click

        public IIconSource? IconSource { get; private set; }

        /// <summary>
        /// Controls visibility of the tray icon. Setting to false removes it from the tray.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value != _isVisible)
                {
                    _isVisible = value;
                    Update();
                    Trace.WriteLine($"ShellNotifyIcon IsVisible: {_isVisible}");
                }
            }
        }

        private const int WM_CALLBACKMOUSEMSG = User32.WM_USER + 1024;
        private const int ICON_ID = 0;

        private readonly Win32Window _window;
        private readonly DispatcherQueueTimer? _invalidationTimer;
        private bool _isCreated;
        private bool _isVisible;
        private string _tooltip = "Audio Switcher";
        private IntPtr _currentIcon = IntPtr.Zero;
        private bool _isDisposed;
        
        // Deduplication for double button-up messages (Windows 11 issue)
        private bool _hasProcessedButtonUp;

        public ShellNotifyIcon(IIconSource? iconSource = null)
        {
            IconSource = iconSource;
            if (IconSource != null)
            {
                IconSource.Changed += OnIconSourceChanged;
            }

            _window = new Win32Window(OnWindowMessage);
            _window.Initialize();

            // Setup invalidation timer for theme/display changes
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            if (dispatcherQueue != null)
            {
                _invalidationTimer = dispatcherQueue.CreateTimer();
                _invalidationTimer.Interval = TimeSpan.FromMilliseconds(500);
                _invalidationTimer.Tick += OnInvalidationTimerTick;
            }

            // Listen for system events
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        /// <summary>
        /// Shows the tray icon. Call this after configuring icon source and tooltip.
        /// </summary>
        public void Show()
        {
            // Ensure we have some icon to display
            if (_currentIcon == IntPtr.Zero && IconSource?.Current == IntPtr.Zero)
            {
                LoadDefaultIcon();
            }
            IsVisible = true;
        }
        
        private void LoadDefaultIcon()
        {
            try
            {
                // Try to load the app icon from the current executable
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    User32.ExtractIconEx(exePath, 0, out var hLarge, out var hSmall, 1);
                    if (hSmall != IntPtr.Zero)
                    {
                        _currentIcon = hSmall;
                        if (hLarge != IntPtr.Zero) User32.DestroyIcon(hLarge);
                        Trace.WriteLine("ShellNotifyIcon: Loaded default icon from exe");
                        return;
                    }
                    if (hLarge != IntPtr.Zero)
                    {
                        _currentIcon = hLarge;
                        Trace.WriteLine("ShellNotifyIcon: Loaded default large icon from exe");
                        return;
                    }
                }
                
                // Fallback: load a speaker Fluent icon
                var speakerIcon = CreateIconFromFluentGlyph("\uE7F5");
                if (speakerIcon != IntPtr.Zero)
                {
                    _currentIcon = speakerIcon;
                    Trace.WriteLine("ShellNotifyIcon: Loaded fallback Fluent speaker icon");
                    return;
                }
                
                // Final fallback: load a system icon (speaker icon from mmres.dll)
                User32.ExtractIconEx(@"C:\Windows\System32\mmres.dll", -3010, out var hLarge2, out var hSmall2, 1);
                if (hSmall2 != IntPtr.Zero)
                {
                    _currentIcon = hSmall2;
                    if (hLarge2 != IntPtr.Zero) User32.DestroyIcon(hLarge2);
                    Trace.WriteLine("ShellNotifyIcon: Loaded fallback mmres speaker icon");
                    return;
                }
                if (hLarge2 != IntPtr.Zero)
                {
                    _currentIcon = hLarge2;
                    Trace.WriteLine("ShellNotifyIcon: Loaded fallback large mmres speaker icon");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ShellNotifyIcon LoadDefaultIcon failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the tray icon using a Fluent icon glyph (e.g., "\uE7F5" for speakers).
        /// </summary>
        public void UpdateIconFromFluentGlyph(string glyph, string? tooltip = null)
        {
            Trace.WriteLine($"ShellNotifyIcon.UpdateIconFromFluentGlyph: glyph='{glyph}', tooltip='{tooltip}'");
            
            if (!string.IsNullOrEmpty(tooltip))
            {
                _tooltip = tooltip;
            }

            var hIcon = CreateIconFromFluentGlyph(glyph);
            if (hIcon != IntPtr.Zero)
            {
                SetIconHandle(hIcon);
            }
            else
            {
                // Still update tooltip even if icon creation fails
                Update();
            }
        }

        /// <summary>
        /// Creates an HICON from a Fluent icon glyph by rendering it to a bitmap.
        /// </summary>
        private IntPtr CreateIconFromFluentGlyph(string glyph, int size = 32)
        {
            try
            {
                // Use Segoe Fluent Icons (Windows 11) or Segoe MDL2 Assets (Windows 10)
                string fontName = "Segoe Fluent Icons";
                if (!IsFontInstalled(fontName))
                {
                    fontName = "Segoe MDL2 Assets";
                }

                // Create 32-bit ARGB bitmap for proper alpha channel
                using var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using var graphics = Graphics.FromImage(bitmap);
                
                // Use high quality rendering
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                
                // Detect if we should use light or dark icon (based on taskbar theme)
                // Use pure white (255,255,255) or pure black (0,0,0) for crisp icons
                var iconColor = IsLightTaskbar() ? Color.FromArgb(255, 0, 0, 0) : Color.FromArgb(255, 255, 255, 255);
                
                graphics.Clear(Color.Transparent);
                
                // Font size should fill most of the icon area
                float fontSize = size * 0.75f;
                using var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                using var brush = new SolidBrush(iconColor);
                
                // Use StringFormat for proper centering
                using var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                // Draw centered in the bitmap
                var rect = new RectangleF(0, 0, size, size);
                graphics.DrawString(glyph, font, brush, rect, format);

                // Convert to HICON
                return bitmap.GetHicon();
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ShellNotifyIcon CreateIconFromFluentGlyph failed: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        private bool IsFontInstalled(string fontName)
        {
            using var testFont = new Font(fontName, 10, FontStyle.Regular, GraphicsUnit.Pixel);
            return testFont.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsLightTaskbar()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key != null)
                {
                    var value = key.GetValue("SystemUsesLightTheme");
                    if (value is int lightTheme)
                    {
                        return lightTheme == 1;
                    }
                }
            }
            catch { }
            return false; // Default to dark (white icons)
        }

        /// <summary>
        /// Hides and removes the tray icon.
        /// </summary>
        public void Hide()
        {
            IsVisible = false;
        }

        /// <summary>
        /// Sets the icon source for theme-reactive icons.
        /// </summary>
        public void SetIconSource(IIconSource source)
        {
            if (IconSource != null)
            {
                IconSource.Changed -= OnIconSourceChanged;
            }
            
            IconSource = source;
            IconSource.Changed += OnIconSourceChanged;
            Update();
        }

        /// <summary>
        /// Sets the tooltip text shown on hover.
        /// </summary>
        public void SetTooltip(string text)
        {
            _tooltip = text;
            Update();
        }

        /// <summary>
        /// Updates the icon from a DLL resource path (e.g., "@%SystemRoot%\System32\mmres.dll,-3004").
        /// </summary>
        public void UpdateIconFromResource(string iconPath, string? tooltip = null)
        {
            Trace.WriteLine($"ShellNotifyIcon.UpdateIconFromResource: path='{iconPath}', tooltip='{tooltip}'");
            
            if (!string.IsNullOrEmpty(tooltip))
            {
                _tooltip = tooltip;
            }

            IntPtr hIconSmall = IntPtr.Zero;
            IntPtr hIconLarge = IntPtr.Zero;
            bool iconLoaded = false;

            try
            {
                if (!string.IsNullOrEmpty(iconPath))
                {
                    string file = iconPath;
                    int index = 0;

                    // Parse @dll,-id format
                    if (iconPath.StartsWith("@"))
                    {
                        var parts = iconPath.Substring(1).Split(',');
                        if (parts.Length == 2)
                        {
                            file = Environment.ExpandEnvironmentVariables(parts[0]);
                            if (int.TryParse(parts[1], out int id))
                            {
                                index = id;
                            }
                        }
                    }

                    Trace.WriteLine($"ShellNotifyIcon: Extracting icon from '{file}' at index {index}");
                    int count = User32.ExtractIconEx(file, index, out hIconLarge, out hIconSmall, 1);
                    Trace.WriteLine($"ShellNotifyIcon: ExtractIconEx returned {count}, hIconSmall={hIconSmall}, hIconLarge={hIconLarge}");
                    
                    if (count > 0 && hIconSmall != IntPtr.Zero)
                    {
                        iconLoaded = true;
                        if (hIconLarge != IntPtr.Zero)
                        {
                            User32.DestroyIcon(hIconLarge);
                        }
                    }
                    else if (count > 0 && hIconLarge != IntPtr.Zero)
                    {
                        hIconSmall = hIconLarge;
                        iconLoaded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"ShellNotifyIcon UpdateIconFromResource failed: {ex.Message}");
            }

            if (iconLoaded)
            {
                SetIconHandle(hIconSmall);
            }
            else
            {
                // Even if icon failed, still update the tooltip
                Update();
            }
        }

        /// <summary>
        /// Directly sets the icon handle (takes ownership of the HICON).
        /// </summary>
        public void SetIconHandle(IntPtr hIcon)
        {
            if (_currentIcon != IntPtr.Zero && _currentIcon != hIcon)
            {
                User32.DestroyIcon(_currentIcon);
            }
            _currentIcon = hIcon;
            Update();
        }

        /// <summary>
        /// Requests focus for the tray icon (for keyboard navigation).
        /// </summary>
        public void SetFocus()
        {
            Trace.WriteLine("ShellNotifyIcon SetFocus");
            var data = MakeNotifyIconData();
            if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_SETFOCUS, ref data))
            {
                Trace.WriteLine($"ShellNotifyIcon NIM_SETFOCUS failed: {Marshal.GetLastWin32Error()}");
            }
        }

        private Shell32.NOTIFYICONDATAW MakeNotifyIconData()
        {
            var iconHandle = IconSource?.Current ?? _currentIcon;
            
            return new Shell32.NOTIFYICONDATAW
            {
                cbSize = Marshal.SizeOf(typeof(Shell32.NOTIFYICONDATAW)),
                hWnd = _window.Handle,
                uID = ICON_ID,
                uFlags = Shell32.NotifyIconFlags.NIF_MESSAGE | 
                         Shell32.NotifyIconFlags.NIF_ICON | 
                         Shell32.NotifyIconFlags.NIF_TIP |
                         Shell32.NotifyIconFlags.NIF_SHOWTIP,
                uCallbackMessage = WM_CALLBACKMOUSEMSG,
                hIcon = iconHandle,
                szTip = _tooltip.Length > 127 ? _tooltip.Substring(0, 127) : _tooltip
            };
        }

        private void Update()
        {
            var data = MakeNotifyIconData();
            
            if (_isVisible)
            {
                if (_isCreated)
                {
                    if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_MODIFY, ref data))
                    {
                        // Modification failed - shell may have restarted
                        Trace.WriteLine($"ShellNotifyIcon NIM_MODIFY failed: {Marshal.GetLastWin32Error()}, recreating...");
                        _isCreated = false;
                        Update(); // Recursive call to NIM_ADD
                    }
                }
                else
                {
                    if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_ADD, ref data))
                    {
                        Trace.WriteLine($"ShellNotifyIcon NIM_ADD failed: {Marshal.GetLastWin32Error()}");
                    }
                    else
                    {
                        _isCreated = true;
                        
                        // Set version for enhanced message behavior
                        data.uTimeoutOrVersion = Shell32.NOTIFYICON_VERSION_4;
                        if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_SETVERSION, ref data))
                        {
                            Trace.WriteLine($"ShellNotifyIcon NIM_SETVERSION failed: {Marshal.GetLastWin32Error()}");
                        }
                    }
                }
            }
            else if (_isCreated)
            {
                if (!Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_DELETE, ref data))
                {
                    Trace.WriteLine($"ShellNotifyIcon NIM_DELETE failed: {Marshal.GetLastWin32Error()}");
                }
                _isCreated = false;
            }
        }

        private void OnWindowMessage(Win32Window.Message msg)
        {
            if (msg.Msg == WM_CALLBACKMOUSEMSG)
            {
                HandleTrayCallback(msg);
            }
            else if (msg.Msg == Shell32.WM_TASKBARCREATED)
            {
                // Taskbar restarted (e.g., explorer.exe crash), recreate icon
                Trace.WriteLine("ShellNotifyIcon: Taskbar recreated, re-adding icon");
                _isCreated = false;
                if (_isVisible)
                {
                    Update();
                }
            }
            else if (msg.Msg == User32.WM_SETTINGCHANGE)
            {
                ScheduleDelayedInvalidation();
            }
        }

        private void HandleTrayCallback(Win32Window.Message msg)
        {
            short notification = (short)(msg.LParam.ToInt64() & 0xFFFF);
            
            switch (notification)
            {
                case (short)Shell32.NotifyIconNotification.NIN_SELECT:
                case User32.WM_LBUTTONUP:
                    // Deduplicate double WM_LBUTTONUP on Windows 11
                    if (!_hasProcessedButtonUp)
                    {
                        _hasProcessedButtonUp = true;
                        
                        // Reset flag after a short delay
                        var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                        dispatcherQueue?.TryEnqueue(() => _hasProcessedButtonUp = false);
                        
                        PrimaryInvoke?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                    
                case (short)Shell32.NotifyIconNotification.NIN_KEYSELECT:
                    PrimaryInvoke?.Invoke(this, EventArgs.Empty);
                    break;
                    
                case User32.WM_MBUTTONUP:
                    TertiaryInvoke?.Invoke(this, EventArgs.Empty);
                    break;
                    
                case User32.WM_CONTEXTMENU:
                case User32.WM_RBUTTONUP:
                    var point = new Point(
                        (short)msg.WParam.ToInt32(),
                        msg.WParam.ToInt32() >> 16);
                    SecondaryInvoke?.Invoke(this, new SecondaryInvokeArgs { Point = point });
                    break;
            }
        }

        private void OnIconSourceChanged(IIconSource source)
        {
            Update();
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            ScheduleDelayedInvalidation();
        }

        private void OnUserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            if (e.Category == Microsoft.Win32.UserPreferenceCategory.General ||
                e.Category == Microsoft.Win32.UserPreferenceCategory.VisualStyle)
            {
                ScheduleDelayedInvalidation();
            }
        }

        private void ScheduleDelayedInvalidation()
        {
            IconSource?.CheckForUpdate();
            _invalidationTimer?.Start();
        }

        private void OnInvalidationTimerTick(DispatcherQueueTimer sender, object args)
        {
            _invalidationTimer?.Stop();
            Update();
            IconSource?.CheckForUpdate();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
                Microsoft.Win32.SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
                
                _invalidationTimer?.Stop();
                
                // Remove the icon from the tray
                if (_isCreated)
                {
                    var data = MakeNotifyIconData();
                    Shell32.Shell_NotifyIconW(Shell32.NotifyIconMessage.NIM_DELETE, ref data);
                    _isCreated = false;
                }
                
                // Cleanup icon handle
                if (_currentIcon != IntPtr.Zero)
                {
                    User32.DestroyIcon(_currentIcon);
                    _currentIcon = IntPtr.Zero;
                }
                
                _window.Dispose();
                _isDisposed = true;
            }
        }
    }
}
