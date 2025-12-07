using System;
using System.Runtime.InteropServices;

namespace AudioSwitcher.Core.Interop
{
    /// <summary>
    /// A reusable Win32 message-only window wrapper for handling native Windows messages.
    /// Based on EarTrumpet's Win32Window pattern.
    /// </summary>
    public class Win32Window : IDisposable
    {
        private const string WindowClassName = "AudioSwitcherMessageWindow";
        private static int _windowCount = 0;
        
        private readonly WndProcDelegate _wndProcDelegate;
        private readonly Action<Message> _messageHandler;
        private IntPtr _hWnd;
        private bool _isDisposed;
        
        public IntPtr Handle => _hWnd;
        
        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        
        public struct Message
        {
            public IntPtr HWnd;
            public uint Msg;
            public IntPtr WParam;
            public IntPtr LParam;
        }
        
        public Win32Window(Action<Message> messageHandler)
        {
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _wndProcDelegate = WndProc;
        }
        
        public void Initialize()
        {
            string className = $"{WindowClassName}_{_windowCount++}";
            
            var wndClass = new WNDCLASS
            {
                style = 0,
                lpfnWndProc = _wndProcDelegate,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = GetModuleHandle(null),
                hIcon = IntPtr.Zero,
                hCursor = IntPtr.Zero,
                hbrBackground = IntPtr.Zero,
                lpszMenuName = "",
                lpszClassName = className
            };

            if (RegisterClass(ref wndClass) == 0)
            {
                System.Diagnostics.Trace.WriteLine($"Win32Window RegisterClass failed: {Marshal.GetLastWin32Error()}");
            }

            // Create message-only window (HWND_MESSAGE = -3)
            _hWnd = CreateWindowEx(
                0,
                className,
                "AudioSwitcherMsgWindow",
                0,
                0, 0, 0, 0,
                new IntPtr(-3), // HWND_MESSAGE
                IntPtr.Zero,
                GetModuleHandle(null),
                IntPtr.Zero
            );

            if (_hWnd == IntPtr.Zero)
            {
                System.Diagnostics.Trace.WriteLine($"Win32Window CreateWindowEx failed: {Marshal.GetLastWin32Error()}");
                
                // Fallback: try creating a regular hidden window
                _hWnd = CreateWindowEx(
                    0,
                    className,
                    "AudioSwitcherMsgWindow",
                    0,
                    0, 0, 0, 0,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    GetModuleHandle(null),
                    IntPtr.Zero
                );
            }
        }
        
        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            _messageHandler(new Message
            {
                HWnd = hWnd,
                Msg = msg,
                WParam = wParam,
                LParam = lParam
            });
            
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }
        
        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (_hWnd != IntPtr.Zero)
                {
                    DestroyWindow(_hWnd);
                    _hWnd = IntPtr.Zero;
                }
                _isDisposed = true;
            }
        }
        
        #region P/Invoke
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WNDCLASS
        {
            public uint style;
            public WndProcDelegate lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszClassName;
        }
        
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
            int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);
        
        #endregion
    }
}
