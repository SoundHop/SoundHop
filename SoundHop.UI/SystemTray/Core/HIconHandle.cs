using System;
using System.Runtime.InteropServices;
using SoundHop.UI.SystemTray.Interfaces;

namespace SoundHop.UI.SystemTray.Core
{
    public class HIconHandle : IIconFile
    {
        private readonly IntPtr _handle;
        private readonly bool _owned;

        public HIconHandle(IntPtr handle, bool owned = false)
        {
            _handle = handle;
            _owned = owned;
        }

        public nint Handle => _handle;

        public void Dispose()
        {
            if (_owned && _handle != IntPtr.Zero)
            {
                DestroyIcon(_handle);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}
