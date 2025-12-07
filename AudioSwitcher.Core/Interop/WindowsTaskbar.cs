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

        #endregion
    }
}
