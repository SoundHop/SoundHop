using System;
using System.Runtime.InteropServices;

namespace SoundHop.Core.Com
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PropertyKey
    {
        public Guid fmtid;
        public uint pid;

        // PKEY_Device_FriendlyName: {a45c254e-df1c-4efd-8020-67d146a850e0}, 14
        public static readonly PropertyKey FriendlyName = new PropertyKey { fmtid = new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), pid = 14 };
        
        // PKEY_Device_IconPath: {259abffc-50a7-47ce-af08-68c9a7d73366}, 12
        public static readonly PropertyKey IconPath = new PropertyKey { fmtid = new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), pid = 12 };
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariant
    {
        [FieldOffset(0)]
        public ushort vt;
        [FieldOffset(2)]
        public ushort wReserved1;
        [FieldOffset(4)]
        public ushort wReserved2;
        [FieldOffset(6)]
        public ushort wReserved3;
        [FieldOffset(8)]
        public IntPtr pwszVal; // VT_LPWSTR
        [FieldOffset(8)]
        public int lVal;       // VT_I4
        [FieldOffset(8)]
        public uint ulVal;     // VT_UI4
    }
}
