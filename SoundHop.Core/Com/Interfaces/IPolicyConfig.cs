using System;
using System.Runtime.InteropServices;

namespace SoundHop.Core.Com.Interfaces
{
    [ComImport]
    [Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPolicyConfig
    {
        [PreserveSig]
        int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out IntPtr ppFormat);
        [PreserveSig]
        int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int bDefault, out IntPtr ppFormat);
        [PreserveSig]
        int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName);
        [PreserveSig]
        int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pEndpointFormat, IntPtr mixFormat);
        [PreserveSig]
        int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int bDefault, out long pmftDefaultPeriod, out long pmftMinimumPeriod);
        [PreserveSig]
        int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, long pmftPeriod);
        [PreserveSig]
        int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out IntPtr pMode);
        [PreserveSig]
        int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr mode);
        [PreserveSig]
        int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, ref PropertyKey pKey, out PropVariant pv);
        [PreserveSig]
        int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, ref PropertyKey pKey, ref PropVariant pv);
        [PreserveSig]
        int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, ERole role);
        [PreserveSig]
        int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int bVisible);
    }
}
