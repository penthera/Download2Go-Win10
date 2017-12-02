using Penthera.VirtuosoClient.Public;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;


namespace VirtuosoClient.TestHarness.Common
{

    /// <summary>
    /// Class AssetStatusFormatter.
    /// Convert the status of the asset to an object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class AssetStatusFormatter : IValueConverter
    {
        /// <summary>
        /// Converts the status of the asset.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((EAssetStatus)value)
            {
                case EAssetStatus.kVDE_Download_None:
                    return "None";
                case EAssetStatus.kVDE_Download_Pending:
                    return "Pending";
                case EAssetStatus.kVDE_Download_Preparing:
                    return "Preparing";
                case EAssetStatus.kVDE_Download_Attaching:
                    return "Attaching";
                case EAssetStatus.kVDE_Download_Ready:
                    return "Ready";
                case EAssetStatus.kVDE_Download_Active:
                    return "Active";
                case EAssetStatus.kVDE_Download_Blocked:
                    return "Blocked";
                case EAssetStatus.kVDE_Download_Paused_Stopped:
                    return "Paused or Stopped";
                case EAssetStatus.KVDE_Download_Error:
                    return "Error";
                case EAssetStatus.kVDE_Download_Expired:
                    return "Expired";
                case EAssetStatus.kVDE_Download_NotAvailable:
                    return "Not Valid";
                case EAssetStatus.kVDE_Download_Complete:
                    return "Complete";
                default:
                    return "INVALID STATUS";
            }
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class AssetErrorFormatter.
    /// Converts the error of the download to an object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class AssetErrorFormatter : IValueConverter
    {
        /// <summary>
        /// Converts the error of the download.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((EDownloadError)value)
            {
                case EDownloadError.kVDE_Download_No_Error:
                    return "None Error";
                case EDownloadError.kVDE_Download_Network_Error:
                    return "Network Error";
                case EDownloadError.kVDE_Download_Error_FileSize:
                    return "Size Error";
                case EDownloadError.kVDE_Download_Error_MimeType:
                    return "MimeType Error";
                case EDownloadError.kVDE_Download_Error_Denied_Account:
                    return "MDA Error";
                case EDownloadError.kVDE_Download_Error_Denied_Assets:
                    return "MAD Error";
                case EDownloadError.kVDE_Download_Error_MaxPermitted_Assets:
                    return "MPD Error";
                case EDownloadError.KVDE_Download_Error_Corrupt:
                    return "Corrupt File Error";
                case EDownloadError.kVDE_Download_Max_Retries_Exceeded:
                    return "Max Retires Error";
                default:
                    return "Error Not Registered!!!";
            }
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class VirtuosoClientStatusFormatter.
    /// Converts the status of the client to an object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class VirtuosoClientStatusFormatter : IValueConverter
    {

        /// <summary>
        /// Converts the status of the client.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string state = "unknown";
            switch ((VirtuosoClientStatus)value)
            {
                case VirtuosoClientStatus.kVC_Downloading:
                    state = "Downloading";
                    break;
                case VirtuosoClientStatus.kVC_Idle:
                    state = "Idle";
                    break;
                case VirtuosoClientStatus.kVC_Disabled:
                    state = "Disabled";
                    break;
                case VirtuosoClientStatus.kVC_Blocked:
                    state = "Blocked";
                    break;
                case VirtuosoClientStatus.kVC_Errors:
                    state = "Errored";
                    break;
                case VirtuosoClientStatus.kVC_AuthenticationFailure:
                    state = "Auth Failure";
                    break;
                case VirtuosoClientStatus.kVC_AuthenticationExpired:
                    state = "Auth Expired";
                    break;
                case VirtuosoClientStatus.kVC_Unlicensed:
                    state = "Unlicensed";
                    break;
                case VirtuosoClientStatus.kVC_Unknown:
                default:
                    //do nothing alread set as unknown
                    break;
            }
            return state;

        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class VirtuosoClientEnabledFormatter.
    /// Converts if client is enabled to an object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class VirtuosoClientEnabledFormatter : IValueConverter
    {
        /// <summary>
        /// Enum Parameters
        /// </summary>
        public enum Parameters
        {
            /// <summary>
            /// The application bar label
            /// </summary>
            APP_BAR_LABEL,
            /// <summary>
            /// The application bar icon
            /// </summary>
            APP_BAR_ICON,
            /// <summary>
            /// The default label
            /// </summary>
            DEFAULT_LABEL
        }

        /// <summary>
        /// Converts if client is enabled to an object string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool _enabled = (bool)value;
            Parameters param = Parameters.DEFAULT_LABEL;
            if (parameter != null)
            {
                param = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);
            }
            switch (param)
            {
                case Parameters.APP_BAR_ICON:
                    return new SymbolIcon(_enabled ? Symbol.Stop : Symbol.Play);
                case Parameters.APP_BAR_LABEL:
                    return _enabled ? "Pause" : "Start";
                default:
                    {
                        return _enabled ? "Pause Downloads" : "Start Downloads";
                    }

            }
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class VirtuosoClientEnvironmentStatusFormatter.
    /// Converts the status of the client to an object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class VirtuosoClientEnvironmentStatusFormatter : IValueConverter
    {
        /// <summary>
        /// Converts the status of the client.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool _ok = (bool)value;

            if (_ok)
                return "OK";
            else
                return "NOT OK";
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class UnlimitedFormatter.
    /// Converts -1 value to 'Unlimited' object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class UnlimitedFormatter : IValueConverter
    {
        /// <summary>
        /// Converts -1 value to 'Unlimited'
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((long)value <= -1 || (long)value == long.MaxValue) ? "Unlimited" : value.ToString();
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class ExpirySpanFormatter.
    /// Converts invalid values to 'Never' object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class ExpirySpanFormatter : IValueConverter
    {
        /// <summary>
        /// Converts invalid values to 'Unlimited'
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((long)value <= -1 || (long)value == long.MaxValue) ? "Never" : value.ToString()+" sec";
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class ExpiryFormatter.
    /// Converts invalid values to 'Never' object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class ExpiryFormatter : IValueConverter
    {
        /// <summary>
        /// Converts invalid values to 'Unlimited'
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((DateTime)value == DateTime.MaxValue || value == null)
                return "Never";

            TimeZoneInfo tz = System.TimeZoneInfo.Local;
            DateTime localTime = System.TimeZoneInfo.ConvertTime((DateTime)value, tz);
            string retVal = localTime.ToString();
            return retVal;
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class FilePathToUriConverter.
    /// Converts the File Path to an URI object
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class FilePathToUriConverter : IValueConverter
    {
        /// <summary>
        /// Converts the File Path to an URI object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            String fp = (string)value;
            if (fp != null)
            {
                return new Uri(fp);
            }
            return null;
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class DiskStorageFormatter.
    /// Converts the number of bytes to correspondent size
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class DiskStorageFormatter : IValueConverter
    {
        /// <summary>
        /// Converts the number of bytes to correspondent size.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double _bytes = Double.Parse(((ulong)value).ToString());
            double _kbytes = _bytes / 1024.0;
            double _mbytes = _kbytes / 1024.0;
            double _gbytes = _mbytes / 1024.0;

            if (_gbytes >= 1.0)
                return _gbytes.ToString("F2") + " GB";
            else if (_mbytes >= 1.0)
                return _mbytes.ToString("F2") + " MB";
            else if (_kbytes >= 1.0)
                return _kbytes.ToString("F2") + " KB";
            else
                return _bytes.ToString("F2") + " Bytes";
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// Class DeviceConverter.
    /// Converts the data of the device to an object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class DeviceConverter : IValueConverter
    {
        /// <summary>
        /// Converts the data of the device.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (parameter as string)
            {
                case "Id":
                    return string.Format("id: {0}", value.ToString());
                case "Nickname":
                    return string.Format("name: {0}", value.ToString());
                case "CurrentDevice":
                    return string.Format("this device: {0}", value.ToString());
                case "Modified":
                    DateTime EpochStartUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    return string.Format("modified: {0}", EpochStartUtc.AddSeconds((long)value));
                case "LastSyncDate":
                    return string.Format("last sync: {0}", value.ToString());
                case "Unregister":
                    return !(bool)value;
                case "Enable":
                    return (bool)value ? "Disable" : "Enable";
                default:
                    return "unknown property";
            }
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Class AssetPercentFormatter.
    /// Converts the percentage of the downloaded asset to an object string
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Data.IValueConverter" />
    public class AssetPercentFormatter : IValueConverter
    {
        /// <summary>
        /// Converts the percentage of the downloaded asset.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double v = (double)(value ?? 0);
            return string.Format("{0}%", ((int)(v * 100)));
        }

        /// <summary>
        /// Converts back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}




