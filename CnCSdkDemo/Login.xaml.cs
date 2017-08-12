using Penthera.VirtuosoClient.Public;
using Penthera.VirtuosoClient.Public.Events;
using System;
using VirtuosoClient.TestHarness.Common;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace VirtuosoClient.TestHarness
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    using static App;
    public sealed partial class Login : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public Login()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            DefaultViewModel["default_url"] = Config.BASE_URL;
            DefaultViewModel["default_user"] = "testUser";
            DefaultViewModel["login_error"] = "";
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {

            VirtuosoClientStatus status = VClient.Status;
            if (status == VirtuosoClientStatus.kVC_AuthenticationFailure)
                setAuthenticationFailure();
            else if (status != VirtuosoClientStatus.kVC_Unknown && VClient.Backplane.BackplaneSettings.UserID != null)
            {
                //handle returning to the page should not happen. Page should be removed from backstack
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Login_Btn.IsEnabled = true;
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Login_Btn.IsEnabled = true;
            navigationHelper.OnNavigatedFrom(e);
            //var cacheSize = ((rootFrame)).CacheSize;
            //((rootFrame)).CacheSize = 0;
            //((rootFrame)).CacheSize = cacheSize;
        }

        #endregion

        // The login process might take a little while.  For simplicity, rather than using a modal progress or some
        // other UI mechansim to allow the user to see what's going on, just prevent them from rapidly tapping the login
        // button.
        private bool _loggingIn = false;
        private void Login_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (_loggingIn)
            {
                Login_Btn.IsEnabled = true;
                return;
            }


            Login_Btn.IsEnabled = false;
            _loggingIn = true;

            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "Login button click");
            VClient.AuthenticationUpdated += Client_AuthenticationChanged;
            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "Registered status change event handler");
            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "calling startup");
            VClient.StartupAsync(
                                (string)DefaultViewModel["default_url"],
                                (string)DefaultViewModel["default_user"],
                                "default_external_id",
                                Config.PRIVATE_KEY, Config.PUBLIC_KEY).AsAsyncAction().Completed = (isender, iargs) =>
                                    {
                                        _loggingIn = false;
                                    };
        }

        /// <summary>
        /// Handles the AuthenticationChanged event of the Client control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AuthenticationEventArgs"/> instance containing the event data.</param>
        /// <exception cref="Exception"></exception>
        private void Client_AuthenticationChanged(object sender, AuthenticationEventArgs e)
        {
            VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug,
                "Login.VirtuosoClientStatusChange [{0}]", e.Status);
            switch (e.Status)
            {
                case AuthenticationStatus.Authentication_Failure:
                    Login_Btn.IsEnabled = true;
                    _loggingIn = false;
                    setAuthenticationFailure();
                    break;

                default:
                    VClient.AuthenticationUpdated -= Client_AuthenticationChanged;

                    if (!Frame.Navigate(typeof(HubPage)))
                    {
                        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                    }
                    
                    break;
            }
        }

        private void setAuthenticationFailure()
        {
            DefaultViewModel["login_error"] = "Authentication failed: Please try Again.";
        }
    }
}
