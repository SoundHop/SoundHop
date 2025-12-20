using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AudioSwitcher.Core.Interop
{
    /// <summary>
    /// Helper for detecting Windows taskbar position, size, and state.
    /// Adapted from EarTrumpet (https://github.com/File-New-Project/EarTrumpet) - MIT License.
    /// </summary>
    public static class WindowsTaskbar
    {
        public enum Position
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        public struct State
        {
            public Position Location;
            public RECT TaskbarBounds;
            public RECT WorkArea;
            public bool IsAutoHideEnabled;
            public uint Dpi;

            public bool IsHorizontal => Location == Position.Bottom || Location == Position.Top;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rect;
            public int lParam;
        }

        private enum AppBarMessage : uint
        {
            New = 0,
            Remove = 1,
            QueryPos = 2,
            SetPos = 3,
            GetState = 4,
            GetTaskbarPos = 5,
            Activate = 6,
            GetAutoHideBar = 7,
            SetAutoHideBar = 8,
            WindowPosChanged = 9,
            SetState = 10
        }

        [Flags]
        private enum AppBarState
        {
            AutoHide = 0x01,
            AlwaysOnTop = 0x02
        }

        /// <summary>
        /// Gets the current taskbar state including position, size, and auto-hide status.
        /// </summary>
        public static State Current
        {
            get
            {
                var hWnd = GetTaskbarHwnd();
                var state = new State();

                var appBarData = new APPBARDATA
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                    hWnd = hWnd
                };

                // Get taskbar position using SHAppBarMessage (handles auto-hide correctly)
                if (SHAppBarMessage(AppBarMessage.GetTaskbarPos, ref appBarData) != UIntPtr.Zero)
                {
                    state.TaskbarBounds = appBarData.rect;
                    state.Location = (Position)appBarData.uEdge;
                }
                else
                {
                    // Fallback: get window rect directly
                    GetWindowRect(hWnd, out state.TaskbarBounds);
                    
                    // Determine position based on taskbar bounds
                    SystemParametersInfo(SPI_GETWORKAREA, 0, out RECT workArea, 0);
                    if (state.TaskbarBounds.Top >= workArea.Bottom)
                        state.Location = Position.Bottom;
                    else if (state.TaskbarBounds.Bottom <= workArea.Top)
                        state.Location = Position.Top;
                    else if (state.TaskbarBounds.Left >= workArea.Right)
                        state.Location = Position.Right;
                    else
                        state.Location = Position.Left;
                }

                // Get auto-hide state
                var appBarState = (AppBarState)(int)SHAppBarMessage(AppBarMessage.GetState, ref appBarData);
                state.IsAutoHideEnabled = appBarState.HasFlag(AppBarState.AutoHide);

                // Get work area (screen minus taskbar)
                SystemParametersInfo(SPI_GETWORKAREA, 0, out state.WorkArea, 0);

                // Get DPI
                state.Dpi = GetDpiForWindow(hWnd);
                if (state.Dpi == 0) state.Dpi = 96;

                Trace.WriteLine($"WindowsTaskbar: Location={state.Location}, AutoHide={state.IsAutoHideEnabled}, Bounds={state.TaskbarBounds.Left},{state.TaskbarBounds.Top},{state.TaskbarBounds.Right},{state.TaskbarBounds.Bottom}");
                return state;
            }
        }

        /// <summary>
        /// Gets the taskbar window handle.
        /// </summary>
        public static IntPtr GetTaskbarHwnd() => FindWindow("Shell_TrayWnd", null);

        /// <summary>
        /// Gets taskbar state based on cursor position - works with multi-monitor setups including DisplayFusion. Though a bit buggy.
        /// </summary>
        public static State GetStateFromCursor()
        {
            // Get cursor position
            GetCursorPos(out POINT cursorPos);
            
            // Find the monitor that contains the cursor
            IntPtr hMonitor = MonitorFromPoint(cursorPos, MONITOR_DEFAULTTONEAREST);
            
            var state = new State();
            
            // Check if primary taskbar is auto-hide
            var primaryTaskbar = GetTaskbarHwnd();
            var appBarData = new APPBARDATA { cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)), hWnd = primaryTaskbar };
            var appBarState = (AppBarState)(int)SHAppBarMessage(AppBarMessage.GetState, ref appBarData);
            state.IsAutoHideEnabled = appBarState.HasFlag(AppBarState.AutoHide);
            
            // Get monitor info (includes work area for THIS monitor)
            var monitorInfo = new MONITORINFO { cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO)) };
            if (GetMonitorInfo(hMonitor, ref monitorInfo))
            {
                var monitor = monitorInfo.rcMonitor;
                var work = monitorInfo.rcWork;
                
                // Check if work area equals monitor bounds (indicates either no taskbar or auto-hide taskbar)
                bool workAreaEqualsMonitor = (work.Left == monitor.Left && work.Right == monitor.Right && 
                                               work.Top == monitor.Top && work.Bottom == monitor.Bottom);
                
                // Check if cursor is on the primary monitor (where Windows taskbar lives)
                IntPtr primaryMonitor = MonitorFromWindow(primaryTaskbar, MONITOR_DEFAULTTONEAREST);
                bool isOnPrimaryMonitor = (hMonitor == primaryMonitor);
                
                if (workAreaEqualsMonitor && state.IsAutoHideEnabled && isOnPrimaryMonitor)
                {
                    // Auto-hide taskbar on primary monitor - use primary taskbar info
                    var primaryState = Current;
                    state.Location = primaryState.Location;
                    state.TaskbarBounds = primaryState.TaskbarBounds;
                    state.WorkArea = monitorInfo.rcMonitor; // Use full monitor as work area
                    
                    Trace.WriteLine($"WindowsTaskbar.GetStateFromCursor: Auto-hide primary detected, using primary taskbar bounds");
                }
                else
                {
                    state.WorkArea = work;
                    
                    // Determine taskbar position by comparing work area to monitor bounds
                    if (work.Bottom < monitor.Bottom)
                        state.Location = Position.Bottom;
                    else if (work.Top > monitor.Top)
                        state.Location = Position.Top;
                    else if (work.Right < monitor.Right)
                        state.Location = Position.Right;
                    else if (work.Left > monitor.Left)
                        state.Location = Position.Left;
                    else
                        state.Location = Position.Bottom; // Default if no taskbar detected
                    
                    // Calculate approximate taskbar bounds based on the difference
                    switch (state.Location)
                    {
                        case Position.Bottom:
                            state.TaskbarBounds = new RECT { Left = work.Left, Top = work.Bottom, Right = work.Right, Bottom = monitor.Bottom };
                            break;
                        case Position.Top:
                            state.TaskbarBounds = new RECT { Left = work.Left, Top = monitor.Top, Right = work.Right, Bottom = work.Top };
                            break;
                        case Position.Left:
                            state.TaskbarBounds = new RECT { Left = monitor.Left, Top = work.Top, Right = work.Left, Bottom = work.Bottom };
                            break;
                        case Position.Right:
                            state.TaskbarBounds = new RECT { Left = work.Right, Top = work.Top, Right = monitor.Right, Bottom = work.Bottom };
                            break;
                    }
                }
            }
            else
            {
                // Fallback to primary taskbar if GetMonitorInfo fails
                var primary = Current;
                state.WorkArea = primary.WorkArea;
                state.Location = primary.Location;
                state.TaskbarBounds = primary.TaskbarBounds;
            }
            
            // Get DPI for this monitor
            state.Dpi = GetDpiForMonitor(hMonitor);
            if (state.Dpi == 0) state.Dpi = 96;

            Trace.WriteLine($"WindowsTaskbar.GetStateFromCursor: cursor=({cursorPos.X},{cursorPos.Y}), Location={state.Location}, AutoHide={state.IsAutoHideEnabled}, WorkArea={state.WorkArea.Left},{state.WorkArea.Top},{state.WorkArea.Right},{state.WorkArea.Bottom}");
            return state;
        }

        private static uint GetDpiForMonitor(IntPtr hMonitor)
        {
            try
            {
                if (GetDpiForMonitorInternal(hMonitor, 0, out uint dpiX, out uint dpiY) == 0)
                    return dpiX;
            }
            catch { }
            return 96;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        #region P/Invoke

        private const int SPI_GETWORKAREA = 0x0030;

        [DllImport("shell32.dll")]
        private static extern UIntPtr SHAppBarMessage(AppBarMessage dwMessage, ref APPBARDATA pData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(int uiAction, int uiParam, out RECT pvParam, int fWinIni);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitorInternal(IntPtr hMonitor, int dpiType, out uint dpiX, out uint dpiY);

        #endregion
    }
}
