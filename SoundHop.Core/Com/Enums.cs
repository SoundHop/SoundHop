using System;

namespace SoundHop.Core.Com
{
    public enum EDataFlow
    {
        Render = 0,
        Capture = 1,
        All = 2
    }

    public enum ERole
    {
        Console = 0,
        Multimedia = 1,
        Communications = 2
    }

    [Flags]
    public enum DeviceState : uint
    {
        Active = 1,
        Disabled = 2,
        NotPresent = 4,
        Unplugged = 8,
        All = 15
    }

    public enum StorageAccessMode
    {
        Read = 0,
        Write = 1,
        ReadWrite = 2
    }
}
