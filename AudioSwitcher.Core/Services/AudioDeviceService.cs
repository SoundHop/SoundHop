using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AudioSwitcher.Core.Com;
using AudioSwitcher.Core.Com.Interfaces;
using AudioSwitcher.Core.Models;

namespace AudioSwitcher.Core.Services
{
    public class AudioDeviceService : IDisposable
    {
        private IMMDeviceEnumerator? _notificationEnumerator;
        private DeviceNotificationClient? _notificationClient;
        public event Action? DevicesChanged;

        public AudioDeviceService()
        {
             InitializeNotifications();
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetCountDelegate(IntPtr self, out uint count);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ItemDelegate(IntPtr self, uint nDevice, out IntPtr ppDevice);
        
        [DllImport("ole32.dll")]
        private static extern int PropVariantClear(ref PropVariant pvar);

        public List<AudioDevice> GetPlaybackDevices(DeviceState stateFilter = DeviceState.Active)
        {
            return GetDevices(EDataFlow.Render, stateFilter);
        }

        public List<AudioDevice> GetCaptureDevices(DeviceState stateFilter = DeviceState.Active)
        {
            return GetDevices(EDataFlow.Capture, stateFilter);
        }

        private List<AudioDevice> GetDevices(EDataFlow dataFlow, DeviceState stateFilter)
        {
            var devices = new List<AudioDevice>();
            IMMDeviceEnumerator? enumerator = null;
            IMMDeviceCollection? collection = null;

            try
            {
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                string? defaultDeviceId = null;
                string? defaultCommsDeviceId = null;
                
                try
                {
                    enumerator.GetDefaultAudioEndpoint(dataFlow, ERole.Multimedia, out IMMDevice defaultDevice);
                    defaultDevice.GetId(out defaultDeviceId);
                    Marshal.ReleaseComObject(defaultDevice);
                }
                catch { /* Ignore if no default device */ }

                try
                {
                    enumerator.GetDefaultAudioEndpoint(dataFlow, ERole.Communications, out IMMDevice defaultCommsDevice);
                    defaultCommsDevice.GetId(out defaultCommsDeviceId);
                    Marshal.ReleaseComObject(defaultCommsDevice);
                }
                catch { /* Ignore if no default comms device */ }

                // Manual marshaling to bypass possible IID cast issues
                IntPtr collectionPtr = IntPtr.Zero;
                int hr = enumerator.EnumAudioEndpoints(dataFlow, (uint)stateFilter, out collectionPtr);
                
                if (hr == 0 && collectionPtr != IntPtr.Zero)
                {
                    IntPtr vptr = Marshal.ReadIntPtr(collectionPtr);
                    
                    // GetCount (Slot 3)
                    IntPtr getCountPtr = Marshal.ReadIntPtr(vptr, 3 * IntPtr.Size);
                    var getCountDelegate = Marshal.GetDelegateForFunctionPointer<GetCountDelegate>(getCountPtr);
                    
                    uint count = 0;
                    getCountDelegate(collectionPtr, out count);

                    // Item (Slot 4)
                    IntPtr itemPtr = Marshal.ReadIntPtr(vptr, 4 * IntPtr.Size);
                    var itemDelegate = Marshal.GetDelegateForFunctionPointer<ItemDelegate>(itemPtr);

                    for (uint i = 0; i < count; i++)
                    {
                        IMMDevice? device = null;
                        try 
                        {
                            IntPtr devicePtr = IntPtr.Zero;
                            itemDelegate(collectionPtr, i, out devicePtr);
                            
                            if (devicePtr != IntPtr.Zero)
                            {
                                device = (IMMDevice)Marshal.GetObjectForIUnknown(devicePtr);
                                Marshal.Release(devicePtr);
                                
                                device.GetId(out string id);
                                device.GetState(out DeviceState deviceState);
                                string name = GetDeviceProperty(device, PropertyKey.FriendlyName);
                                string iconPath = GetDeviceProperty(device, PropertyKey.IconPath);
                                
                                devices.Add(new AudioDevice
                                {
                                    Id = id,
                                    Name = name,
                                    IsDefault = id == defaultDeviceId,
                                    IsDefaultComms = id == defaultCommsDeviceId,
                                    IconPath = ParseIconPath(iconPath),
                                    State = (uint)deviceState,
                                    IsInput = dataFlow == EDataFlow.Capture
                                });
                            }
                        }
                        catch
                        {
                            // Ignore specific device errors
                        }
                        finally
                        {
                            if (device != null) Marshal.ReleaseComObject(device);
                        }
                    }
                    Marshal.Release(collectionPtr);
                }
            }
            catch
            {
                // Ignore failure to enumerate
            }
            finally
            {
                if (collection != null) Marshal.ReleaseComObject(collection);
                if (enumerator != null) Marshal.ReleaseComObject(enumerator);
            }
            
            return devices;
        }

        public void SetDefaultDevice(string deviceId)
        {
            IPolicyConfig? policyConfig = null;
            try
            {
                // Creating PolicyConfigClient which implements IPolicyConfig
                policyConfig = (IPolicyConfig)new PolicyConfigClient();
                policyConfig.SetDefaultEndpoint(deviceId, ERole.Multimedia);
                policyConfig.SetDefaultEndpoint(deviceId, ERole.Console);
                policyConfig.SetDefaultEndpoint(deviceId, ERole.Communications);
            }
            finally
            {
                if (policyConfig != null) Marshal.ReleaseComObject(policyConfig);
            }
        }

        public void SetDefaultCommunicationDevice(string deviceId)
        {
            IPolicyConfig? policyConfig = null;
            try
            {
                policyConfig = (IPolicyConfig)new PolicyConfigClient();
                policyConfig.SetDefaultEndpoint(deviceId, ERole.Communications);
            }
            finally
            {
                if (policyConfig != null) Marshal.ReleaseComObject(policyConfig);
            }
        }

        public void SetDeviceEnabled(string deviceId, bool enabled)
        {
            IPolicyConfig? policyConfig = null;
            try
            {
                policyConfig = (IPolicyConfig)new PolicyConfigClient();
                policyConfig.SetEndpointVisibility(deviceId, enabled ? 1 : 0);
            }
            finally
            {
                if (policyConfig != null) Marshal.ReleaseComObject(policyConfig);
            }
        }

        public void OpenDeviceProperties(string deviceId)
        {
            // Open the Sound control panel
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "mmsys.cpl",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Fallback: open Windows Settings sound page
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "ms-settings:sound",
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }
        
        private string GetDeviceProperty(IMMDevice device, PropertyKey propertyKey)
        {
            IPropertyStore? store = null;
            PropVariant pv = new PropVariant();
            try
            {
                device.OpenPropertyStore(StorageAccessMode.Read, out store);
                store.GetValue(ref propertyKey, out pv);
                return Marshal.PtrToStringUni(pv.pwszVal) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                PropVariantClear(ref pv);
                if (store != null) Marshal.ReleaseComObject(store);
            }
        }

        private string ParseIconPath(string rawPath)
        {
            if (string.IsNullOrEmpty(rawPath)) return "";
            return rawPath; 
        }

        private void InitializeNotifications()
        {
            try
            {
                _notificationEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                _notificationClient = new DeviceNotificationClient();
                _notificationClient.DeviceChanged += OnDeviceChanged;
                _notificationClient.DefaultDeviceChanged += OnDeviceChanged; // Treat as generic change
                _notificationEnumerator.RegisterEndpointNotificationCallback(_notificationClient);
            }
            catch { /* Handle/Log error */ }
        }

        private void OnDeviceChanged()
        {
            DevicesChanged?.Invoke();
        }

        public void Dispose()
        {
            if (_notificationEnumerator != null && _notificationClient != null)
            {
                try { _notificationEnumerator.UnregisterEndpointNotificationCallback(_notificationClient); } catch {}
                if (_notificationEnumerator != null) Marshal.ReleaseComObject(_notificationEnumerator);
                _notificationEnumerator = null;
                _notificationClient = null;
            }
        }
    }
}
