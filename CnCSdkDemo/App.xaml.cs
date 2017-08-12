using Penthera.VirtuosoClient.Public;
using Penthera.VirtuosoClient.Public.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using VirtuosoClient.TestHarness.Common;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace VirtuosoClient.TestHarness
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        internal static IVirtuosoClient VClient = VirtuosoClientFactory.ClientInstance();
        internal static IVirtuosoSettings VSettings = VClient.Settings;
        internal static Frame RootFrame { get; set; }
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// Initializes the singleton instance of the <see cref="App"/> class. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            UnhandledException += App_UnhandledException;
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            //var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif

            Frame RootFrame = Window.Current.Content as Frame;
            Type startPage = await StartPage();
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (RootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                RootFrame = new Frame();
                RootFrame.Navigated += onNavigated;
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(RootFrame, "AppFrame");

                RootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated || e.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = RootFrame;

                // Each time a navigation event occurs, update the Back button's visibility
                //Allow system to decide on type of back button
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    RootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;

            }

            if (RootFrame.Content == null)
            {
                //Type startPage = await StartPage();
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (RootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in RootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                RootFrame.ContentTransitions = null;
                RootFrame.Navigated += this.RootFrame_FirstNavigated;
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!RootFrame.Navigate(startPage, e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
#else
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!RootFrame.Navigate(startPage, e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
#endif
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void onNavigated(object sender, NavigationEventArgs e)
        {
            Debug.WriteLine("Navigation {0} from {1}", e.Uri, e.SourcePageType.FullName);
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        async Task<Type> StartPage()
        {
            //starts client instance
            IVirtuosoClient client = VirtuosoClientFactory.ClientInstance();
            //handle remove wipe events
            client.RemoteWipeComplete += client_RemoteWipeComplete;
            //handle backplane permission events
            client.BackplanePermissionReceived += Client_BackplanePermissionReceived;
            client.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug,
                "App setting start page.");
            String user = client.Backplane.BackplaneSettings.UserID;
            VirtuosoClientStatus status = client.Status;
            if (status == VirtuosoClientStatus.kVC_AuthenticationFailure || status == VirtuosoClientStatus.kVC_Unknown || user == null)
            {
                return typeof(Login);
            }
            // call startup and direct to main app page
            await client.StartupAsync(
                                      client.Backplane.BackplaneSettings.BackplaneUrl,
                                      user,
                                      client.Backplane.BackplaneSettings.ExternalDeviceID,
                                      Config.PRIVATE_KEY, Config.PUBLIC_KEY);
            return typeof(HubPage);
        }

        private void Client_BackplanePermissionReceived(IVirtuosoClient sender, BackplanePermissionEventArgs e)
        {
            IBackplanePermission ibp = e.BackplanePermission;
            sender.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug,
                "Received Backplane Permission for Asset {0} with download permitted: {1}.", e.Asset, ibp.DownloadAllowed);
        }

        void client_RemoteWipeComplete(object sender, EventArgs e)
        {
            SuspensionManager.CleanState();
            // Normally, you would want to reset the application state to an un-logged in view, for instance, a Login page.
            // For simplicity, we'll simply exit the app here.
            Application.Current.Exit();
        }

        public async Task UnregisterDevice()
        {
            if (await VirtuosoClientFactory.ClientInstance().UnregisterThisDevice())
            {
                SuspensionManager.CleanState();
                // Normally, you would want to reset the application state to an un-logged in view, for instance, a Login page.
                // For simplicity, we'll simply exit the app here.
                Application.Current.Exit();
            }
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var logger = VirtuosoClientFactory.ClientInstance().VirtuosoLogger;
                StringBuilder sb = new StringBuilder();
                sb.Append("FATAL EXCEPTION");
                sb.AppendLine();
                sb.Append(e.Message);
                sb.AppendLine();
                sb.Append(e.Exception.StackTrace);
                if (e.Exception.InnerException != null)
                {
                    sb.Append("InnerException:");
                    sb.AppendLine();
                    sb.Append(e.Exception.InnerException.Message);
                    sb.AppendLine();
                    if (e.Exception.InnerException.StackTrace != null)
                        sb.Append(e.Exception.InnerException.StackTrace);
                }
                Exception baseex = e.Exception.GetBaseException();
                if (baseex != null)
                {
                    sb.Append("BaseException");
                    sb.AppendLine();
                    sb.Append(baseex.Message);
                    sb.AppendLine();
                    if (baseex.StackTrace != null)
                        sb.Append(baseex.StackTrace);
                }
                logger.WriteLine(VirtuosoLoggingLevel.Error, sb.ToString());
            }
            catch (Exception) { }
        }
    }
}
