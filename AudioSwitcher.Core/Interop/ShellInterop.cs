using System;
using System.Runtime.InteropServices;

namespace AudioSwitcher.Core.Interop
{
    /// <summary>
    /// Shell32 P/Invoke declarations for shell notification icons.
    /// </summary>
    public static class Shell32
    {
        public const int NOTIFYICON_VERSION_4 = 4;
        
        // WM_TASKBARCREATED is dynamically registered
        public static readonly uint WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");
        
        public enum NotifyIconMessage : uint
        {
            NIM_ADD = 0x00000000,
            NIM_MODIFY = 0x00000001,
            NIM_DELETE = 0x00000002,
            NIM_SETFOCUS = 0x00000003,
            NIM_SETVERSION = 0x00000004
        }
        
        [Flags]
        public enum NotifyIconFlags : uint
        {
            NIF_MESSAGE = 0x00000001,
            NIF_ICON = 0x00000002,
            NIF_TIP = 0x00000004,
            NIF_STATE = 0x00000008,
            NIF_INFO = 0x00000010,
            NIF_GUID = 0x00000020,
            NIF_REALTIME = 0x00000040,
            NIF_SHOWTIP = 0x00000080
        }
        
        /// <summary>
        /// Notification icon notification messages (sent in lParam low word with NOTIFYICON_VERSION_4).
        /// </summary>
        public enum NotifyIconNotification : short
        {
            NIN_SELECT = 0x0400, // WM_USER + 0
            NIN_KEYSELECT = 0x0401,
            NIN_BALLOONSHOW = 0x0402,
            NIN_BALLOONHIDE = 0x0403,
            NIN_BALLOONTIMEOUT = 0x0404,
            NIN_BALLOONUSERCLICK = 0x0405,
            NIN_POPUPOPEN = 0x0406,
            NIN_POPUPCLOSE = 0x0407
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct NOTIFYICONDATAW
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public NotifyIconFlags uFlags;
            public uint uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;
            public int dwState;
            public int dwStateMask;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public uint uTimeoutOrVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public int dwInfoFlags;
            public Guid guidItem;
            public IntPtr hBalloonIcon;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct NOTIFYICONIDENTIFIER
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uID;
            public Guid guidItem;
        }
        
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool Shell_NotifyIconW(NotifyIconMessage dwMessage, ref NOTIFYICONDATAW lpData);
        
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int Shell_NotifyIconGetRect(ref NOTIFYICONIDENTIFIER identifier, out RECT iconLocation);
        
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint RegisterWindowMessage(string lpString);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            
            public int Width => Right - Left;
            public int Height => Bottom - Top;
            
            public bool Contains(int x, int y) =>
                x >= Left && x <= Right && y >= Top && y <= Bottom;
        }
    }
    
    /// <summary>
    /// User32 P/Invoke declarations for window messages.
    /// </summary>
    public static class User32
    {
        public const int WM_USER = 0x0400;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_CONTEXTMENU = 0x007B;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_SETTINGCHANGE = 0x001A;
        public const int SPI_SETWORKAREA = 0x002F;
        
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);
        
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int ExtractIconEx(string lpszFile, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, int nIcons);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
    }
}
