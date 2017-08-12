using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VirtuosoClient.TestHarness
{
    using Penthera.VirtuosoClient.Public;
    using VirtuosoClient.TestHarness.Common;
    using VirtuosoClient.TestHarness.Data;
    using Windows.System.Threading;
    using static App;
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AssetPage : Page
    {
        private NavigationHelper navigationHelper;
        private IAsset Asset { get; set; }
        private ObservableDictionary _defaultModel = new ObservableDictionary();
        public ObservableDictionary DefaultModel { get { return _defaultModel; } }
        //public DisplayMonitor DisplayMonitor { get; private set; }

        public AssetPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
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
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var guid = (Guid)e.NavigationParameter;
            QATestDataSource.GetAssetAsync(guid).AsAsyncOperation<IAsset>().Completed = (sender2, args) =>
            {
                Asset = sender2.GetResults();

                //set source to a local variable
                var source = Asset.MediaPlayBackSource;
                //check if source exist
                if (source != null)
                {
                    //set source to player
                    mediaPlayerElement.SetPlaybackSource(source);
                }
                DefaultModel["Asset"] = Asset;
            };
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
        
        private void Delete_Asset_Click(object sender, RoutedEventArgs e)
        {
            //set player source as null to void crash
            mediaPlayerElement.Source = null;

            //Thread to delete selected asset
            //We suggest to use threadpool to wait delete conclude before continue
            ThreadPool.RunAsync(async (workitem) => await VClient.DeleteAssetAsync(Asset));

            this.NavigationHelper.GoBack();
        }

        private void Reset_Errors_Click(object sender, RoutedEventArgs e)
        {
            //set player source as null to void crash
            mediaPlayerElement.Source = null;

            Asset.ResetErrorState();
            this.NavigationHelper.GoBack();
        }
    }
}
