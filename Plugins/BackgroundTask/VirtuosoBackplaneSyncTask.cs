using Penthera.VirtuosoClient.Public;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;

namespace Penthera.VirtuosoClient.BackgroundTask
{
    public sealed class VirtuosoBackplaneSyncTask : IBackgroundTask
    {
        BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        volatile bool _cancelRequested = false;
        
        //
        // The Run method is the entry point of a background task.
        //
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            VirtuosoClientFactory.ClientInstance().VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug,"Background " + taskInstance.Task.Name + " Starting...");

            //
            // Associate a cancellation handler with the background task.
            //
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            await VirtuosoClientFactory.ClientInstance().SyncWithBackplaneAsync();

            //Push Notification
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
            RawNotification notification = (RawNotification)taskInstance.TriggerDetails;
            string content = notification.Content;
            Debug.WriteLine(content);
            _deferral.Complete();


            //ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
            //string taskName = taskInstance.Task.Name;

            //Debug.WriteLine("Background " + taskName + " starting...");

            //// Store the content received from the notification so it can be retrieved from the UI.
            //RawNotification notification1 = (RawNotification)taskInstance.TriggerDetails;
            //settings.Values[taskName] = notification1.Content;
            //Windows.Data.Xml.Dom.XmlDocument doc = new XmlDocument();

            //var toast = new ToastNotification(doc);
            //var center = ToastNotificationManager.CreateToastNotifier();
            //center.Show(toast);


            //Debug.WriteLine("Background " + taskName + " completed!");

        }

        // 
        // Handles background task cancellation.
        //
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //
            // Indicate that the background task is canceled.
            //
            _cancelRequested = true;
            _cancelReason = reason;

            VirtuosoClientFactory.ClientInstance().VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "Background " + sender.Task.Name + " Cancel Requested...Reason: " + reason.ToString());
        }
    }
}
