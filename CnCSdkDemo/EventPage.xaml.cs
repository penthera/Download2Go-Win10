using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VirtuosoClient.TestHarness
{
    using Penthera.VirtuosoClient.Public;
    using VirtuosoClient.TestHarness.Common;
    using Windows.UI.Popups;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    using static App;
    public sealed partial class EventPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        IAsyncOperation<IUICommand> PopUpCommand = null;
        private Object _setupPadlock = new Object();
        IVirtuosoClient client = VirtuosoClientFactory.ClientInstance();

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }
        public EventPage()
        {
            this.InitializeComponent();
            navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            DefaultViewModel["Events"] = VClient.EventList().Result;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var _events = (IEvent)DefaultViewModel["Events"];
            var result = await client.UpdateEvent(_events);
            if (result == true)
            {
                MessageDialog msg = new MessageDialog("Event details have updated successfully");
                msg.ShowAsync();
            }
        }
        private async void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DeviceList));
        }

        private async void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HubPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingPage));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var _events = (IEvent)DefaultViewModel["Events"];
            client.UpdateEvent(_events);
            this.navigationHelper.OnNavigatedFrom(e);
        }
    }
}
