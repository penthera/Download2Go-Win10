using System;
using System.Diagnostics;
using Windows.Media.Protection.PlayReady;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

namespace VirtuosoClient.TestHarness
{
    using Microsoft.Media.AdaptiveStreaming;
    using Microsoft.PlayerFramework.Adaptive;
    using Penthera.VirtuosoClient.Public;
    using System.Collections.Generic;
    using VirtuosoClient.TestHarness.Common;
    using VirtuosoClient.TestHarness.Data;
    using Windows.Foundation;
    using Windows.Foundation.Collections;
    using Windows.Media;
    using Windows.Media.Protection;
    using Windows.System.Threading;
    using Windows.UI.Xaml.Media;
    using static App;
    using System.Threading.Tasks;
    using Windows.System.Display;
    using System.IO;
    using Windows.Storage.Streams;
    using Windows.Media.Core;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AssetPage : Page
    {
        private NavigationHelper navigationHelper;
        private double timeAtPlayStart;
        MediaElementState lastReportedState = MediaElementState.Buffering;
        private bool sentPlayStart = false;
        private IAsset Asset { get; set; }
        private IAdaptiveSourceManager adaptiveSourceManager;
        private IRandomAccessStream stream = null;
        private DisplayRequest _displayRequest;
        
        private ObservableDictionary _defaultModel = new ObservableDictionary();
        public ObservableDictionary DefaultModel { get { return _defaultModel; } }
        //public DisplayMonitor DisplayMonitor { get; private set; }

        public AssetPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            _displayRequest = new DisplayRequest();
        }

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
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
            var guid = (Guid)e.NavigationParameter;
            QATestDataSource.GetAssetAsync(guid).AsAsyncOperation<IAsset>().Completed = (sender2, args) => {
                Asset = sender2.GetResults();
                Asset.ExpireHandler += asset_AssetExpired;
                if (Asset.AssetType == EAssetType.Single && Asset.Status == EAssetStatus.kVDE_Download_Complete) {
                    //set stream to a local variable
                    string fileName = Asset.FQFileName;
                    try
                    {
                        stream = File.Open(fileName, FileMode.Open, FileAccess.Read).AsRandomAccessStream();
                    }
                    catch( Exception ex )
                    {
                        stream = null;
                    }

                    //check if source exist
                    if (stream != null) {
                        //set source to player
                        Player.SetSource(stream, Asset.ReceivedMime);
                        Player.CurrentStateChanged += media_MediaStateChanged;
                    }
                } else {
                    // If you are using PlayReady DRM, you may need to implement/modify this method.  See below for details.
                    //InitializeMediaProtectionManager();

                    Asset.PrepareForPlaybackInPlayer(Player);
                    Player.CurrentStateChanged += media_MediaStateChanged;
                }
                DefaultModel["Asset"] = Asset;

            };
        }

        private void asset_AssetExpired (object sender, EventArgs e)
        {
            IAsset asset = (IAsset)DefaultModel["Asset"];
            if (asset.Expired)
            {
                try
                {
                    asset.CleanupAfterPlayback();
                    Player.Stop();
                    Player.Dispose();
                }
                catch (Exception) { }
            }
        }

        private void media_MediaStateChanged(object sender, RoutedEventArgs e)
        {
            IAsset asset = (IAsset)DefaultModel["Asset"];
            if (asset.Expired)
                return;

            double timeAtPlayStop = 0;
            switch (Player.CurrentState)
            {
                case MediaElementState.Buffering:
                    break;
                case MediaElementState.Closed:
                    break;
                case MediaElementState.Opening:
                    break;
                case MediaElementState.Paused:
                    if (lastReportedState == MediaElementState.Playing)
                    {
                        sentPlayStart = false;
                        timeAtPlayStop = DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        VirtuosoClientFactory.ClientInstance().VirtuosoLogger.LogPlayStop((IAsset)DefaultModel["Asset"], (ulong)((timeAtPlayStop - timeAtPlayStart) / 1000));
                    }
                    break;
                case MediaElementState.Playing:
                    if (!sentPlayStart)
                    {
                        sentPlayStart = true;
                        timeAtPlayStart = DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        VirtuosoClientFactory.ClientInstance().VirtuosoLogger.LogPlayStart((IAsset)DefaultModel["Asset"]);
                    }
                    break;
                case MediaElementState.Stopped:
                    if (lastReportedState == MediaElementState.Playing)
                    {
                        sentPlayStart = false;
                        timeAtPlayStop = DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        VirtuosoClientFactory.ClientInstance().VirtuosoLogger.LogPlayStop((IAsset)DefaultModel["Asset"], (ulong)((timeAtPlayStop - timeAtPlayStart) / 1000));
                    }
                    break;
                default:
                    break;
            }
            lastReportedState = Player.CurrentState;
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
            _displayRequest.RequestActive();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            IAsset asset = (IAsset)DefaultModel["Asset"];
            asset.CleanupAfterPlayback();
            if( stream != null )
            {
                stream.Dispose();
            }
            this.navigationHelper.OnNavigatedFrom(e);
            Asset.ExpireHandler -= asset_AssetExpired;
            _displayRequest.RequestRelease();
        }

        #endregion

        #region Button Event Handlers
        private void Delete_Asset_Click(object sender, RoutedEventArgs e)
        {
            IAsset asset = (IAsset)DefaultModel["Asset"];
            asset.CleanupAfterPlayback();
            if (stream != null)
            {
                stream.Dispose();
            }

            //set player source as null to void crash
            Player.Dispose();

            //Thread to delete selected asset
            //We suggest to use threadpool to wait delete conclude before continue
            ThreadPool.RunAsync(async (workitem) => await VClient.DeleteAssetAsync(asset, DeleteReason.User));

            this.Frame.Navigate(typeof(HubPage));
        }

        private void Pause_Asset_Click(object sender, RoutedEventArgs e) {
            VClient.PauseActivity();
            this.NavigationHelper.GoBack();
        }

        private void Resume_Asset_Click(object sender, RoutedEventArgs e) {
            VClient.ResumeActivity();
            this.NavigationHelper.GoBack();
        }

        private void Reset_Errors_Click(object sender, RoutedEventArgs e)
        {
            IAsset asset = (IAsset)DefaultModel["Asset"];
            asset.CleanupAfterPlayback();

            //set player source as null to void crash
            Player.Dispose();

            Asset.ResetErrorState();
            this.NavigationHelper.GoBack();
        }
        #endregion

        /// <summary>
        /// Initializes the PlayReady protection manager.  This method contains example code only.  You will most likely need to customize
        /// this code to function with your particular DRM configuration.
        /// </summary>
        private void InitializeMediaProtectionManager()
        {
            var mediaProtectionManager = new MediaProtectionManager();
            mediaProtectionManager.ComponentLoadFailed += OnMediaProtectionManagerComponentLoadFailed;
            mediaProtectionManager.ServiceRequested += OnMediaProtectionManagerServiceRequested;

            // Set up the container GUID for the CFF format, see http://uvdemystified.com/uvfaq.html#3.       
            // The GUID represents MPEG DASH Content Protection using Microsoft PlayReady, see http://dashif.org/identifiers/protection/
            mediaProtectionManager.Properties["Windows.Media.Protection.MediaProtectionContainerGuid"] = "{9A04F079-9840-4286-AB92-E65BE0885F95}";

            // Set up the drm layer to use. Hardware DRM is the default, but not all older hardware supports this
            var supportsHardwareDrm = PlayReadyStatics.CheckSupportedHardware(PlayReadyHardwareDRMFeatures.HardwareDRM);
            if (!supportsHardwareDrm)
            {
                mediaProtectionManager.Properties["Windows.Media.Protection.UseSoftwareProtectionLayer"] = true;
            }

            // Set up the content protection manager so it uses the PlayReady Input Trust Authority (ITA) for the relevant media sources
            // The MediaProtectionSystemId GUID is format and case sensitive, see https://msdn.microsoft.com/en-us/library/windows.media.protection.mediaprotectionmanager.properties.aspx
            var cpsystems = new PropertySet();
            cpsystems[PlayReadyStatics.MediaProtectionSystemId.ToString("B").ToUpper()] = "Windows.Media.Protection.PlayReady.PlayReadyWinRTTrustedInput";
            mediaProtectionManager.Properties["Windows.Media.Protection.MediaProtectionSystemIdMapping"] = cpsystems;
            mediaProtectionManager.Properties["Windows.Media.Protection.MediaProtectionSystemId"] = PlayReadyStatics.MediaProtectionSystemId.ToString("B").ToUpper();

            Player.ProtectionManager = mediaProtectionManager;
        }

        private void OnMediaProtectionManagerComponentLoadFailed(MediaProtectionManager sender, ComponentLoadFailedEventArgs e)
        {
            Debug.WriteLine("ProtectionManager ComponentLoadFailed");
            e.Completion.Complete(false);
        }

        private async void OnMediaProtectionManagerServiceRequested(MediaProtectionManager sender, ServiceRequestedEventArgs e)
        {
            Debug.WriteLine("ProtectionManager ServiceRequested");

            var completionNotifier = e.Completion;
            var request = (IPlayReadyServiceRequest)e.Request;

            var result = false;

            if (request.Type == PlayReadyStatics.IndividualizationServiceRequestType)
            {
                result = await PlayReadyLicenseHandler.RequestIndividualizationToken(request as PlayReadyIndividualizationServiceRequest);
            }
            else if (request.Type == PlayReadyStatics.LicenseAcquirerServiceRequestType)
            {
                // NOTE: You might need to set the request.ChallengeCustomData, depending on your Rights Manager.
               // request.Uri = new Uri(arguments.RightsManagerUrl);

                //result = await PlayReadyLicenseHandler.RequestLicense(request as PlayReadyLicenseAcquisitionServiceRequest);
            }

            completionNotifier.Complete(result);
        }

        private void ViewInfo_Click(object sender, RoutedEventArgs e)
        {
            IAsset asset = (IAsset)DefaultModel["Asset"];
            this.Frame.Navigate(typeof(AssetInfoView), asset.Uuid);
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HubPage));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PlayReadyLicenseHandler
    {
        /// <summary>Request a token that identifies the player session.</summary>
        /// <param name="request">The request.</param>
        /// <returns><c>True</c> if successfull, <c>false</c> otherwise.</returns>
        public static async Task<bool> RequestIndividualizationToken(PlayReadyIndividualizationServiceRequest request)
        {
            Debug.WriteLine("ProtectionManager PlayReady Individualization Service Request in progress");

            try
            {
                Debug.WriteLine("Requesting individualization token from {0}", request.Uri);

                await request.BeginServiceRequest();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProtectionManager PlayReady Individualization Service Request failed: " + ex.Message);

                return false;
            }

            Debug.WriteLine("ProtectionManager PlayReady Individualization Service Request successfull");

            return true;
        }

        /// <summary>Request a license for playing a stream.</summary>
        /// <param name="request">The request.</param>
        /// <returns><c>True</c> if successfull, <c>false</c> otherwise.</returns>
        public static async Task<bool> RequestLicense(PlayReadyLicenseAcquisitionServiceRequest request)
        {
            Debug.WriteLine("ProtectionManager PlayReady License Request in progress");

            try
            {
                Debug.WriteLine("Requesting license from {0} with custom data {1}", request.Uri, request.ChallengeCustomData);

                await request.BeginServiceRequest();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProtectionManager PlayReady License Request failed: " + ex.Message);

                return false;
            }

            Debug.WriteLine("ProtectionManager PlayReady License Request successfull");

            return true;
        }
    }
}
