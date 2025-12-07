using System;
using AudioSwitcher.Core.Com;
using AudioSwitcher.Core.Com.Interfaces;

namespace AudioSwitcher.Core.Services
{
    public class DeviceNotificationClient : IMMNotificationClient
    {
        public event Action? DeviceChanged;
        public event Action? DefaultDeviceChanged;

        public int OnDeviceStateChanged(string pwstrDeviceId, uint dwNewState)
        {
            DeviceChanged?.Invoke();
            return 0;
        }

        public int OnDeviceAdded(string pwstrDeviceId)
        {
            DeviceChanged?.Invoke();
            return 0;
        }

        public int OnDeviceRemoved(string pwstrDeviceId)
        {
            DeviceChanged?.Invoke();
            return 0;
        }

        public int OnDefaultDeviceChanged(EDataFlow flow, ERole role, string pwstrDefaultDeviceId)
        {
            if (flow == EDataFlow.Render && role == ERole.Console)
            {
                DefaultDeviceChanged?.Invoke();
            }
            return 0;
        }

        public int OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            // property change might affect friendly name
            DeviceChanged?.Invoke();
            return 0;
        }
    }
}
