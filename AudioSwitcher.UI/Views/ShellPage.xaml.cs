using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace AudioSwitcher.UI.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Select the first item by default
            NavView.SelectedItem = NavView.MenuItems[0];
            NavView_Navigate("OutputDevices", new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                NavView_Navigate("Settings", args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null)
            {
                var navItemTag = args.InvokedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate(string navItemTag, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
        {
            Type _page = null;
            if (navItemTag == "Settings")
            {
                _page = typeof(SettingsPage);
            }
            else
            {
                switch (navItemTag)
                {
                    case "OutputDevices":
                        _page = typeof(OutputDevicesPage);
                        break;
                    case "Hotkeys":
                        _page = typeof(HotkeysPage);
                        break;
                }
            }

            // Get the page type before navigation so you can prevent duplicate
            // entries in the backstack.
            var preNavPageType = ContentFrame.CurrentSourcePageType;

            // Only navigate if the selected page isn't currently loaded.
            if (_page != null && !Type.Equals(preNavPageType, _page))
            {
                ContentFrame.Navigate(_page, null, transitionInfo);
            }
        }
    }
}
