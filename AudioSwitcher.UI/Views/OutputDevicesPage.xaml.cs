using Microsoft.UI.Xaml.Controls;
using AudioSwitcher.UI.ViewModels;

namespace AudioSwitcher.UI.Views
{
    public sealed partial class OutputDevicesPage : Page
    {
        public MainViewModel ViewModel { get; }

        public OutputDevicesPage()
        {
            this.InitializeComponent();
            ViewModel = MainViewModel.Instance;
            ViewModel.Initialize();
            DeviceListControl.ViewModel = ViewModel;
        }
    }
}
