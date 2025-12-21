using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SoundHop.Core.Models;

namespace SoundHop.UI.Services
{
    public class GlobalHotkeyService : IDisposable
    {
        private static GlobalHotkeyService? _instance;
        public static GlobalHotkeyService Instance => _instance ??= new GlobalHotkeyService();

        private readonly IntPtr _hwnd;
        private readonly Dictionary<int, Action> _actions = new();
        private int _currentId;
        
        // CRITICAL: Keep a reference to the delegate to prevent garbage collection!
        private readonly WndProcDelegate _wndProcDelegate;

        private const int WM_HOTKEY = 0x0312;

        public GlobalHotkeyService()
        {
            // Store the delegate so it doesn't get garbage collected
            _wndProcDelegate = WndProc;
            
            // Create a message-only window to receive WM_HOTKEY
            _hwnd = CreateMessageWindow();
        }

        public int Register(KeyModifiers modifiers, int key, Action action)
        {
            _currentId++;
            if (RegisterHotKey(_hwnd, _currentId, (uint)modifiers, (uint)key))
            {
                _actions[_currentId] = action;
                System.Diagnostics.Debug.WriteLine($"Registered hotkey {_currentId}: modifiers={modifiers}, key={key}");
                return _currentId;
            }
            System.Diagnostics.Debug.WriteLine($"Failed to register hotkey: modifiers={modifiers}, key={key}, error={Marshal.GetLastWin32Error()}");
            return 0;
        }

        public void Unregister(int id)
        {
            if (_actions.ContainsKey(id))
            {
                UnregisterHotKey(_hwnd, id);
                _actions.Remove(id);
            }
        }

        public void UnregisterAll()
        {
            foreach (var id in _actions.Keys)
            {
                UnregisterHotKey(_hwnd, id);
            }
            _actions.Clear();
            _currentId = 0;
        }

        private IntPtr CreateMessageWindow()
        {
            string className = "SoundHopHotkeyListener" + Guid.NewGuid().ToString("N").Substring(0, 8);
            
            WNDCLASS wc = new WNDCLASS
            {
                lpfnWndProc = _wndProcDelegate, // Use the stored delegate
                lpszClassName = className
            };

            RegisterClass(ref wc);
            
            return CreateWindowEx(0, className, "", 0, 0, 0, 0, 0, new IntPtr(-3) /* HWND_MESSAGE */, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        private IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                System.Diagnostics.Debug.WriteLine($"WM_HOTKEY received: id={id}");
                if (_actions.TryGetValue(id, out var action))
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Hotkey action error: {ex.Message}");
                    }
                }
            }
            return DefWindowProc(hwnd, msg, wParam, lParam);
        }

        public void Dispose()
        {
            foreach (var id in _actions.Keys)
            {
                UnregisterHotKey(_hwnd, id);
            }
            DestroyWindow(_hwnd);
        }

        // P/Invoke
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
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
            public string lpszMenuName;
            public string lpszClassName;
        }
    }
}
