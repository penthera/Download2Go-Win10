using Penthera.VirtuosoClient.Public;
using Penthera.VirtuosoClient.Public.Events;
using Penthera.VirtuosoClient.Public.License;
using System;
using System.IO;
using VirtuosoClient.TestHarness.Common;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VirtuosoClient.TestHarness
{
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Email;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    using static App;
    public sealed partial class SettingPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        IAsyncOperation<IUICommand> PopUpCommand = null;
        private Object _setupPadlock = new Object();
        public string license_status = "";
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


        /// <summary>
        /// Initializes a new instance of the <see cref="SettingPage"/> class.
        /// </summary>
        public SettingPage()
        {
            InitializeComponent();
            navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;

            //handle CcbDevice Saved
            VClient.VirtuosoCcbDeviceSaved += Instance_VirtuosoCcbDeviceSaved;

            //handle license changed
            VClient.LicenseChanged += Instance_LicenseChanged;

            VClient.AuthenticationUpdated += Instance_BackplaneUpdated;
        }

        /// <summary>
        /// Backplane authentication has completed.  Updated view with potentially new data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_BackplaneUpdated(object sender, AuthenticationEventArgs e)
        {
            NavigationHelper_LoadState(sender, null);
        }

        /// <summary>
        /// Instances of the license changed.
        /// </summary>
        /// <param name="license">The license.</param>
        /// <param name="e">The <see cref="LicenseEventArgs"/> instance containing the event data.</param>
        private void Instance_LicenseChanged(ILicense license, LicenseEventArgs e)
        {
            loadLicenseData(license);
        }

        /// <summary>
        /// Loads the license data.
        /// </summary>
        /// <param name="license">The license.</param>
        private void loadLicenseData(ILicense license)
        {
            DefaultViewModel["License"] = license;
            DateTime? val = license.ExpiryDate;
            DefaultViewModel["LicenseKeyExpiryDate"] = val == null ? "" : (val == DateTime.MaxValue ? "NO expiry" : val.ToString());
            DefaultViewModel["LicenseKeyvalueExpiryDay"] = string.Format("{0}", license.ExpiryDays);
            DefaultViewModel["MaxSdk"] = license.MaxSdkVersion;
            DefaultViewModel["LicenseStatus"] = license.LicenseState;
        }

        /// <summary>
        /// Handles the LoadState event of the NavigationHelper control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LoadStateEventArgs"/> instance containing the event data.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            DefaultViewModel["EngineStatus"] = client.Status;
            DefaultViewModel["NetworkStatus"] = client.NetworkStatusOK;
            DefaultViewModel["DiskStatus"] = client.DiskStatusOK;
            DefaultViewModel["DiskFreeSpace"] = client.DiskFreeSpace;

            DefaultViewModel["Settings"] = client.Settings;

            DefaultViewModel["BackplaneSettings"] = client.Backplane.backplaneSettings;
            DefaultViewModel["Moff"] = client.Backplane.backplaneSettings.MaximumOfflinePeriod == DateTime.MaxValue.Ticks ? "Unlimited" : client.Backplane.backplaneSettings.MaximumOfflinePeriod.ToString();
            DefaultViewModel["MDD"] = client.Backplane.backplaneSettings.MaximumDevicesForDownload == long.MaxValue ? "Unlimited" : client.Backplane.backplaneSettings.MaximumDevicesForDownload.ToString();

            DefaultViewModel["Device"] = client.ThisDevice;

            DefaultViewModel["LogLevel"] = (int)client.VirtuosoLogger.LoggingLevel;
            DefaultViewModel["NetworkLoggingEnabled"] = client.VirtuosoLogger.NetworkLoggingEnabled;
            DefaultViewModel["NetworkLoggingIP"] = client.VirtuosoLogger.NetworkLoggingIP;
            DefaultViewModel["NetworkLoggingPort"] = client.VirtuosoLogger.NetworkLoggingPort;
            loadLicenseData(VClient.License);
            DefaultViewModel["SecureClock"] = client.VirtuosoTime;
            DefaultViewModel["SystemClock"] = DateTime.UtcNow;
            DefaultViewModel["TrustVirtuosoTime"] = client.TrustVirtuosoTime;
            DefaultViewModel["HttpCodeErrorSegment"] = client.Settings.SegmentErrorHTTPCode;

            bool overrideSync = false;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("OverrideSyncLimit"))
                overrideSync = (bool)ApplicationData.Current.LocalSettings.Values["OverrideSyncLimit"];

            DefaultViewModel["OverrideSyncLimit"] = overrideSync;
        }

        /// <summary>
        /// Handles the Click event of the PickFolderButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add(".docx");
            folderPicker.FileTypeFilter.Add(".xlsx");
            folderPicker.FileTypeFilter.Add(".pptx");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                DefaultViewModel["Destination"] = folder.Path;
            }
            else
            {
                //DefaultViewModel["Destination"]=
                // OutputTextBlock.Text = "Operation cancelled.";
            }
        }

        /// <summary>
        /// Handles the OnNetworkLogEnabledChanged event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Selector_OnNetworkLogEnabledChanged(object sender, RoutedEventArgs e)
        {
            CheckBox box = (CheckBox)sender;
            VirtuosoClientFactory.ClientInstance().VirtuosoLogger.NetworkLoggingEnabled = (box.IsChecked == true);
            DefaultViewModel["NetworkLoggingEnabled"] = VirtuosoClientFactory.ClientInstance().VirtuosoLogger.NetworkLoggingEnabled;
        }

        /// <summary>
        /// Handles the OnLogLevelChanged event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Selector_OnLogLevelChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox) == null) return;
            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (item == null) return;
            VirtuosoClientFactory.ClientInstance().VirtuosoLogger.LoggingLevel = (VirtuosoLoggingLevel)int.Parse(item.Tag.ToString());
            DefaultViewModel["LogLevel"] = (int)VirtuosoClientFactory.ClientInstance().VirtuosoLogger.LoggingLevel;
        }

        /// <summary>
        /// Handles the CodeError event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Selector_CodeError(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox) == null) return;
            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (item == null) return;
            VirtuosoClientFactory.ClientInstance().Settings.SegmentErrorHTTPCode = int.Parse(item.Tag.ToString());
            DefaultViewModel["HttpCodeErrorSegment"] = (int)VirtuosoClientFactory.ClientInstance().Settings.SegmentErrorHTTPCode;
        }

        /// <summary>
        /// Handles the Click event of the HyperlinkButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DeviceList));
        }

        private void OverrideSyncLimit_Click(object sender, RoutedEventArgs e)
        {
            CheckBox overrideBox = (CheckBox)sender;
            ApplicationData.Current.LocalSettings.Values["OverrideSyncLimit"] = overrideBox.IsChecked;
            DefaultViewModel["OverrideSyncLimit"] = overrideBox.IsChecked;
        }

        /// <summary>
        /// Handles the VirtuosoCcbDeviceSaved event of the Instance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="VirtuosoDeviceSavedEventArgs"/> instance containing the event data.</param>
        private void Instance_VirtuosoCcbDeviceSaved(object sender, VirtuosoDeviceSavedEventArgs e)
        {
            if (!e.Success)
            {
                MessageDialog popup = new MessageDialog("Device Save Error: " + e.ErrorString);
                popup.ShowAsync();
            }
        }

        /// <summary>
        /// Handles the ItemClick event of the SettingSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void SettingSave_ItemClick(object sender, RoutedEventArgs e)
        {
            client.Settings.Save();
            var dialog = new MessageDialog("Your settings have been saved.");
            await dialog.ShowAsync();

            VirtuosoClientFactory.ClientInstance().VirtuosoLogger.NetworkLoggingIP = (String)DefaultViewModel["NetworkLoggingIP"];
            VirtuosoClientFactory.ClientInstance().VirtuosoLogger.NetworkLoggingPort = (String)DefaultViewModel["NetworkLoggingPort"];
            VirtuosoClientFactory.ClientInstance().VirtuosoLogger.NetworkLoggingEnabled = (bool)DefaultViewModel["NetworkLoggingEnabled"];
        }

        /// <summary>
        /// Handles the Click event of the DeviceButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DeviceList));
        }

        /// <summary>
        /// Handles the Click event of the HomeButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HubPage));
        }

        /// <summary>
        /// Handles the Click event of the ForceSync control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ForceSync_Click(object sender, RoutedEventArgs e)
        {
            await client.SyncWithBackplaneAsync();
        }

        private void Backbutton(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HubPage));
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }


        #endregion

        /// <summary>
        /// Handles the ItemClick event of the Mail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Mail_ItemClick(object sender, RoutedEventArgs e)
        {
            MailZippedLogFiles();
        }


        /// <summary>
        /// Mails the zipped log files.
        /// </summary>
        /// <returns>Void</returns>
        public async Task MailZippedLogFiles()
        {
            try
            {
                StorageFolder sourcefolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("dlogs");
                if (sourcefolder != null)
                {
                    // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", sourcefolder);

                    if (File.Exists(sourcefolder.Path.ToString() + "\\Harness.zip"))
                    {
                        StorageFile filed = await sourcefolder.GetFileAsync("Harness.zip");
                        await filed.DeleteAsync();
                    }
                    await Task.Run(() =>
                    {
                        try
                        {
                            ZipFile.CreateFromDirectory(sourcefolder.Path, $"{sourcefolder.Path}\\Harness.zip",
                                CompressionLevel.Optimal, true);
                        }
                        catch (Exception w)
                        {
                        }
                    });
                }
                EmailMessage emailMessage = new EmailMessage();

                string subject = "CnCSDK - Version: " + VClient.Version + " Logs. ";
                string messageBody = string.Format("Device Info:\n{0}", ToString());
                emailMessage.Body = messageBody;

                emailMessage.Subject = subject;

                StorageFile attachmentFile = await sourcefolder.GetFileAsync("Harness.zip");
                if (attachmentFile != null)
                {
                    var stream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(attachmentFile);
                    var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                             attachmentFile.Name,
                             stream);
                    emailMessage.Attachments.Add(attachment);
                }
                await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
