using Penthera.VirtuosoClient.Public;
using Penthera.VirtuosoClient.Public.Events;
using Penthera.VirtuosoClient.Utilities;
using System;
using System.Threading.Tasks;
using System.Threading;
using VirtuosoClient.TestHarness.Common;
using VirtuosoClient.TestHarness.Data;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;

namespace VirtuosoClient.TestHarness {
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    using static App;
    public sealed partial class HubPage : Page {
        static private class HSSLocalPreferences {
            public const Boolean showPopupForVideoQualityDownload = false;
        }

        private NavigationHelper navigationHelper;
        private EQualityLevel SelectedQuality;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        IAsyncOperation<IUICommand> PopUpCommand = null;
        private Object _setupPadlock = new Object();
        public string license_status = "";

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel {
            get { return defaultViewModel; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public HubPage() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;

            //handle asset size validation warning
            VClient.AssetSizeValidationWarning += Instance_AssetSizeValidationWarning;

            //handle backplane permission
            VClient.BackplanePermissionReceived += Client_BackplanePermissionReceived;

            var autoEvent = new System.Threading.AutoResetEvent(false);
            Timer timer = new Timer(new TimerCallback(RunSyncAsync), autoEvent, (int)VClient.Settings.MinimumBackplaneSyncInterval * 60000, 60);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void RunSyncAsync(object state) {
            AutoResetEvent autoEvent = (AutoResetEvent)state;
            if (VClient.Settings.MinimumBackplaneSyncInterval >= 60) {
                VClient.SyncWithBackplaneAsync();
                VClient.Settings.LastSyncDateTime = DateTime.UtcNow;
            }
            autoEvent.Set();
        }

        /// <summary>
        /// Refresh asset lists as an asynchronous operation.
        /// </summary>
        /// <returns>Void</returns>
        private async Task RefreshAssetListsAsync() {
            await RefreshQueuedAssetsAsync();
            await RefreshCompletedAssetsAsync();
            await RefreshExpiredAssetsAsync();
        }

        /// <summary>
        /// Refresh queued assets as an asynchronous operation.
        /// </summary>
        /// <returns>Void</returns>
        private async Task RefreshQueuedAssetsAsync() {

            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "++ Harness.Refresh Pending Tests");
            var pending = await QATestDataSource.GetPendingAssetsAsync();
            DefaultViewModel["PendingTests"] = pending;
        }
        
        /// <summary>
        /// Refresh completed assets as an asynchronous operation.
        /// </summary>
        /// <returns>Void</returns>
        private async Task RefreshCompletedAssetsAsync() {
            var completed = await QATestDataSource.GetCompletedAssetsAsync();
            DefaultViewModel["CompletedTests"] = completed;
        }

        /// <summary>
        /// Refresh expired assets as an asynchronous operation.
        /// </summary>
        /// <returns>Void</returns>
        private async Task RefreshExpiredAssetsAsync()
        {
            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "++ Harness.Refresh Expired Tests");
            var expired = await QATestDataSource.GetExpiredAssetsAsync();
            DefaultViewModel["ExpiredTests"] = expired;
        }

        /// <summary>
        /// Adds the asset to queue.
        /// </summary>
        /// <param name="test">Asset to be added</param>
        private void AddAssetsToQueue(QATest test) {
            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "++ Harness.AddAssetsToQueueAsync");
            for (var i = 0; i < test.Assets.Count; i++) {
                var testAsset = test.Assets[i];
                IAsset asset = null;
                if (testAsset.SegmentedAssetType == ESegmentedAssetType.NONE) {
                    // Download an MP4.  

                    asset = App.VClient.NewAsset(
                              testAsset.AssetId,
                              testAsset.Title,
                              testAsset.FileUrl,
                              testAsset.Title,
                              testAsset.PublishDate,
                              testAsset.ExpiryDate,
                              testAsset.ExpiryAfterPlay,
                              testAsset.ExpiryAfterDownload
                        );
                } else if (testAsset.SegmentedAssetType == ESegmentedAssetType.HSS) {
                    asset = App.VClient.NewAsset(
                              testAsset.AssetId,
                              testAsset.Title,
                              testAsset.FileUrl,
                              long.MaxValue,
                              long.MaxValue,
                              testAsset.Title,
                              testAsset.PublishDate,
                              testAsset.ExpiryDate,
                              testAsset.ExpiryAfterPlay,
                              testAsset.ExpiryAfterDownload
                        );
                } else {
                    // UNSUPPORTED ASSET TYPE
                }

                if (asset != null) {
                    //adds asset to queue
                    VClient.EnqueueFileAsync(asset).ContinueWith((task) => {
                        if (!task.Result) {
                            MessageDialog popup = new MessageDialog("Unable to add the asset \"" + asset.Description() + "\" to the download queue. This is most likely because the asset, or an asset with the same download URL, is already in the queue.");
                            popup.ShowAsync();
                        }
                    });
                }
            }
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
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_BackplanePermissionReceived(object sender, BackplanePermissionEventArgs e) {
            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "Received Backplane Permission {0} for Asset {1}", e.BackplanePermission, e.Asset.AssetId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async void Instance_AssetSizeValidationWarning(object sender, AssetSizeValidationWarningEventArgs e) {
            EAssetStatus vast = e.Asset.Status;
            String warning = "Warning File Size Mismatch ";
            if (vast == EAssetStatus.kVDE_Download_Complete)
                warning += "on Download Complete: ";
            else
                warning += "during download: ";
            MessageDialog popup = new MessageDialog(warning + e.Asset.AssetId);
            if (PopUpCommand != null) {
                PopUpCommand.Cancel();
            }
            try {
                PopUpCommand = popup.ShowAsync();
            } catch { }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            Task.Run(async () =>
                DefaultViewModel["Sections"] = await QATestDataSource.GetSectionsAsync()
            );
            await RefreshAssetListsAsync();
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        /// <param name="sender">The GridView or ListView
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e) {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((IAsset)e.ClickedItem).Uuid;
            this.Frame.Navigate(typeof(AssetPage), itemId);
        }

        /// <summary>
        /// Handles the OnItemClick event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance containing the event data.</param>
        private async void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e) {
            var item = ((QATest)e.ClickedItem).Clone();

            // We suggest you to disable video download quality pop-up
            if (HSSLocalPreferences.showPopupForVideoQualityDownload && item.Assets[0].AssetType == EAssetType.Segmented) {
                MessageDialog msgbox = new MessageDialog("Which quality do you want to download ?");
                msgbox.Commands.Add(new UICommand("High", new UICommandInvokedHandler(SegmentHandler), EQualityLevel.High));
                msgbox.Commands.Add(new UICommand("Medium", new UICommandInvokedHandler(SegmentHandler), EQualityLevel.Medium));
                msgbox.Commands.Add(new UICommand("Low", new UICommandInvokedHandler(SegmentHandler), EQualityLevel.Low));
                await msgbox.ShowAsync();
                item.Assets[0].Quality = SelectedQuality;
            } else if (item.Assets[0].AssetType == EAssetType.Segmented) {
                item.Assets[0].Quality = EQualityLevel.Medium;
            }

            //Thread to add asset on Queue
            ThreadPool.RunAsync((workitem) => {
                AddAssetsToQueue(item);
            }); // Don't await.

            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "Clicked: {0}", item.Title);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        private async void SegmentHandler(IUICommand command) {
            SelectedQuality = (EQualityLevel)command.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RefreshQueue_Click(object sender, RoutedEventArgs e) {
            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "++ HubPage.Refresh Queque()");
            await RefreshQueuedAssetsAsync();
        }
      
        /// <summary>
        /// Handles the OnLogLevelChanged event of the Selector control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void Selector_OnLogLevelChanged(object sender, SelectionChangedEventArgs e) {
            if ((sender as ComboBox) == null) return;
            var item = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (item == null) return;
            VClient.VirtuosoLogger.LoggingLevel = (VirtuosoLoggingLevel)int.Parse(item.Tag.ToString());
            DefaultViewModel["LogLevel"] = (int)VClient.VirtuosoLogger.LoggingLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingButton_Click(object sender, RoutedEventArgs e) {
            this.Frame.Navigate(typeof(SettingPage));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceButton_Click(object sender, RoutedEventArgs e) {
            this.Frame.Navigate(typeof(DeviceList));
        }

        /// <summary>
        /// Handles the Click event of the ForceSync control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ForceSync_Click(object sender, RoutedEventArgs e) {
            var diff = DateTime.UtcNow.Subtract(VClient.Settings.LastSyncDateTime);

            if (diff.TotalMinutes > VClient.Settings.MinimumBackplaneSyncInterval) {
                await VClient.SyncWithBackplaneAsync();
                VClient.Settings.LastSyncDateTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Accessor to the DeleteAll() method in the public side of the SDK. 
        /// Not used internally.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Public_Delete_All(object sender, RoutedEventArgs e) {
            await VClient.DeleteAll();
        }

        /// <summary>
        /// Handles the Click event of the Clear Queue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ClearQueue_Click(object sender, RoutedEventArgs e) {
            ThreadPool.RunAsync(async (workitem) => VClient.DeletePendingQueue());
            await RefreshQueuedAssetsAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventButton_Click(object sender, RoutedEventArgs e) {
            this.Frame.Navigate(typeof(EventPage));
        }

        /// <summary>
        /// Pauses all activity and removes the currently logged in user's session but not their 
        /// device registration or state.
        /// Redirects back to Login state on success.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogoutButton_Click(object sender, RoutedEventArgs e) {
            // pause downloads.
            VClient.PauseActivity();
            VClient.Logout();
            this.Frame.Navigate(typeof(Login));
        }

        private void GetChannelUriButton_Click(object sender, RoutedEventArgs e) {
            var uri = VClient.Backplane.backplaneSettings.DevicePushToken;
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText( uri);
            Clipboard.SetContent(dataPackage);

            var dialog = new MessageDialog("Device Uri has been copied to your clipboard. Paste it in the request window. : " + uri);
            dialog.ShowAsync();
        }

        private void OutputLogTrace_Click(object sender, RoutedEventArgs e)
        {
            VClient.OutputTraceLog();
        }

        private void ClearLogTrace_Click(object sender, RoutedEventArgs e)
        {
            VClient.ClearTraceLog();
        }

        #endregion
    }
}
