using System;
using System.Runtime.InteropServices;

namespace SoundHop.Core.Com.Interfaces
{
    [ComImport]
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC539238725E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceCollection
    {
        [PreserveSig]
        int GetCount(out uint pcDevices);
        [PreserveSig]
        int Item(uint nDevice, out IMMDevice ppDevice);
    }
}
