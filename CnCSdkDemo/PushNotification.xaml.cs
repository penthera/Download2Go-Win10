using Penthera.VirtuosoClient.Public;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VirtuosoClient.TestHarness
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PushNotification : Page
    {
        internal static IVirtuosoClient VClient = VirtuosoClientFactory.ClientInstance();
        private const string SAMPLE_TASK_NAME = "VirtuosoBackplaneSyncTask";
        private const string SAMPLE_TASK_ENTRY_POINT = "Penthera.VirtuosoClient.BackgroundTask.VirtuosoBackplaneSyncTask";

        public PushNotification()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the PushNotification control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PushNotification_Click(object sender, RoutedEventArgs e)
        {
            string message = PushText.Text;
            SendToastNotification(message, "");
            PushText.Text = "";
        }
        
        /// <summary>
        /// Sends the toast notification.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="imageName">Name of the image.</param>
        public void SendToastNotification(string message, string imageName)
        {
            var notificationXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);
            var toastElements = notificationXml.GetElementsByTagName("text");
            toastElements[0].AppendChild(notificationXml.CreateTextNode(message));
            if (string.IsNullOrEmpty(imageName))
            {
                imageName = @"Assets/Logo.png";
            }
            var imageElement = notificationXml.GetElementsByTagName("image");
            imageElement[0].Attributes[1].NodeValue = imageName;
            var toastNotification = new ToastNotification(notificationXml);
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        /// <summary>
        /// Sends the badge notification.
        /// </summary>
        /// <param name="count">Total amount of notifications</param>
        private void SendBadgeNotification(int count)
        {
            XmlDocument badgeXml =
              BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", count.ToString());
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);

        }

        /// <summary>
        /// Handles the <see cref="E:Completed" /> event.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="args">The <see cref="BackgroundTaskCompletedEventArgs"/> instance containing the event data.</param>
        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            // TODO: Add code that deals with background task completion.
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList textElements = toastXml.GetElementsByTagName("text");
            textElements[0].AppendChild(toastXml.CreateTextNode("Notification - Yeah"));
            textElements[1].AppendChild(toastXml.CreateTextNode("I'm message from your Notification!"));
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));
        }

        /// <summary>
        /// Handles the Click event of the DeviceButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DeviceList));
        }

        /// <summary>
        /// Handles the Click event of the HomeButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void HomeButton_Click(object sender, RoutedEventArgs e)
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
            await VClient.SyncWithBackplaneAsync();
        }
    }
}
