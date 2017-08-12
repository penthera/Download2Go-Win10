using Penthera.VirtuosoClient.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using VirtuosoClient.TestHarness.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VirtuosoClient.TestHarness
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    using static App;
    public sealed partial class DeviceList : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public DeviceList()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return defaultViewModel; }
        }
        

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            DefaultViewModel["DeviceList"] = await App.VClient.UserDeviceList();
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private void ClosePopupClicked(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (StandardPopup.IsOpen) { StandardPopup.IsOpen = false; }
        }

        private void Backbutton(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            this.Frame.Navigate(typeof(HubPage));
        }

        private void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DeviceList));
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HubPage));
        }

        private async void ForceSync_Click(object sender, RoutedEventArgs e)
        {
            await VClient.SyncWithBackplaneAsync();
        }


        /// <summary>
        /// Change NickName
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeNickname_Clicked(object sender, RoutedEventArgs e)
        {
            //get device details
            string _Id = Convert.ToString(((Button)e.OriginalSource).CommandParameter);
            var _nickname = UpdatedNickName.Text;
            
            //select device from device list
            List<IDevice> userDeviceList = (List<IDevice>)DefaultViewModel["DeviceList"];
            var _devicedetail = userDeviceList.Where(x => x.DeviceID == _Id).FirstOrDefault();
            if (_devicedetail != null)
            {
                _devicedetail.NickName = _nickname;
                
                //update device details
                bool result = await App.VClient.UpdateDeviceDetail(_devicedetail);
                if (result == true)
                {
                    //update device list
                    var list = await App.VClient.UserDeviceList();
                    DefaultViewModel["DeviceList"] = list;
                    StandardPopup.IsOpen = false;
                }
            }
        }

        //// Handles the Click event on the Button on the page and opens the Popup. 
        private void ShowPopupOffsetClicked(object sender, RoutedEventArgs e)
        {
            var _Id = ((Button)e.OriginalSource).CommandParameter;
            List<IDevice> userDeviceList = (List<IDevice>)DefaultViewModel["DeviceList"];
            var _devicedetail = userDeviceList.Where(x => x.DeviceID == _Id).FirstOrDefault();

            // open the Popup if it isn't open already 
            if (!StandardPopup.IsOpen)
            {

                DefaultViewModel["NickName"] = _devicedetail.NickName;
                DefaultViewModel["SelectedDeviceId"] = _devicedetail.DeviceID;
                StandardPopup.IsOpen = true;
            }
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingPage));
        }

        /// <summary>
        /// Unregister selected device.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void UnRegisterClicked(object sender, RoutedEventArgs e)
        {
            var _Id = ((Button)e.OriginalSource).CommandParameter;
            List<IDevice> userDeviceList = (List<IDevice>)DefaultViewModel["DeviceList"];

            //select device from device list
            var _devicedetail = userDeviceList.Where(x => x.DeviceID == _Id).FirstOrDefault();
            if (_devicedetail != null)
            {
                //unregister device
                bool result = await App.VClient.DeviceUnRegister(_devicedetail);
                if (result == true)
                {
                    //update device list
                    var list = await App.VClient.UserDeviceList();
                    DefaultViewModel["DeviceList"] = list;
                }
            }
        }

        /// <summary>
        /// Disable selected device.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void DisabledClicked(object sender, RoutedEventArgs e)
        {

            var _Id = ((Button)e.OriginalSource).CommandParameter;
            List<IDevice> userDeviceList = (List<IDevice>)DefaultViewModel["DeviceList"];

            //select device from device list
            var _devicedetail = userDeviceList.Where(x => x.DeviceID == _Id).FirstOrDefault();
            if (_devicedetail != null)
            {
                //Set status of device
                if (_devicedetail.DownloadEnabled == true)
                {
                    _devicedetail.DownloadEnabled = false;
                }
                else
                {
                    _devicedetail.DownloadEnabled = true;
                }

                //update device details
                bool result = await App.VClient.UpdateDeviceDetail(_devicedetail);
                if (result == true)
                {
                    //update device list
                    var list = await App.VClient.UserDeviceList();
                    DefaultViewModel["DeviceList"] = list;
                }
            }

        }
        private void PushNotification_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PushNotification));
        }
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
