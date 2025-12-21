using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace SoundHop.UI.Views
{
    public sealed partial class ShellPage : Page
    {
        private bool _pendingSettingsNavigation = false;
        private bool _pendingHotkeysNavigation = false;

        public ShellPage()
        {
            this.InitializeComponent();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if we have a pending navigation to settings
            if (_pendingSettingsNavigation)
            {
                _pendingSettingsNavigation = false;
                NavView.SelectedItem = NavView.SettingsItem;
                NavView_Navigate("Settings", new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
            }
            else if (_pendingHotkeysNavigation)
            {
                _pendingHotkeysNavigation = false;
                NavigateToHotkeysInternal();
            }
            else
            {
                // Select the first item by default
                NavView.SelectedItem = NavView.MenuItems[0];
                NavView_Navigate("OutputDevices", new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
            }
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
                    case "InputDevices":
                        _page = typeof(InputDevicesPage);
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

        /// <summary>
        /// Navigates to the Settings page and selects the settings item in the nav.
        /// If called before NavView is loaded, sets a flag to navigate on load.
        /// </summary>
        public void NavigateToSettings()
        {
            // If NavView isn't loaded yet, set a flag to navigate when it loads
            if (ContentFrame.CurrentSourcePageType == null)
            {
                _pendingSettingsNavigation = true;
                return;
            }
            
            NavView.SelectedItem = NavView.SettingsItem;
            NavView_Navigate("Settings", new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
        }

        /// <summary>
        /// Navigates to the Hotkeys page.
        /// </summary>
        public void NavigateToHotkeys()
        {
            // If NavView isn't loaded yet, set a flag to navigate when it loads
            if (ContentFrame.CurrentSourcePageType == null)
            {
                _pendingHotkeysNavigation = true;
                return;
            }
            
            NavigateToHotkeysInternal();
        }

        private void NavigateToHotkeysInternal()
        {
            // Find the Hotkeys nav item and select it
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == "Hotkeys")
                {
                    NavView.SelectedItem = navItem;
                    NavView_Navigate("Hotkeys", new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
                    break;
                }
            }
        }
    }
}
