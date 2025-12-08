using Microsoft.UI.Xaml.Controls;
using AudioSwitcher.UI.ViewModels;
using AudioSwitcher.Core.Models;

namespace AudioSwitcher.UI.Views
{
    public sealed partial class OutputDevicesPage : Page
    {
        public MainViewModel ViewModel { get; }

        public OutputDevicesPage()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();
        }

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is AudioDevice selectedDevice)
            {
                 if (selectedDevice.Id != ViewModel.DefaultDevice?.Id)
                 {
                     ViewModel.DefaultDevice = selectedDevice;
                 }
            }
        }
    }
}
