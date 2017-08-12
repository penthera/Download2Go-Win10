using Penthera.VirtuosoClient.Annotations;
using Penthera.VirtuosoClient.Public;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Storage;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace VirtuosoClient.TestHarness.Data
{
    public enum EQualityLevel
    {
        Low,
        Medium,
        High
    }
    /// <summary>
    /// QA Test Asset data model.
    /// </summary>
    public class QATestAsset
    {
        public QATestAsset(String assetId, String title, String fileUrl, String publishDate, String expiryDate,
            String expiryAfterDownload, String expiryAfterPlay, String expectedMD5, String expectedSize,
            String mimeTypes, String metadata, String mediaType)
        {
            AssetId = assetId;
            Title = title;
            FileUrl = fileUrl;
            ExpiryDateTemplate = expiryDate;
            PublishDateTemplate = publishDate;
            ExpiryAfterDownload = expiryAfterDownload == null ? null : (long?)long.Parse(expiryAfterDownload);
            ExpiryAfterPlay = expiryAfterPlay == null ? null : (long?)long.Parse(expiryAfterPlay);
            //   MaxDownloadAccount= maxDownloadAccount==null?null : (long?)long.Parse(maxDownloadAccount);
            ExpectedMD5 = expectedMD5;
            ExpectedSize = expectedSize == null ? null : (ulong?)ulong.Parse(expectedSize);
            Metadata = metadata;
            _mediaType = int.Parse(mediaType);

            if (!String.IsNullOrWhiteSpace(mimeTypes))
            {
                var mt = mimeTypes.Split(',');
                if (mt.Any()) PermittedMimeTypes = new List<string>(mt);
            }
            Quality = EQualityLevel.Medium;
        }

        public QATestAsset(QATestAsset other)
        {
            AssetId = other.AssetId;
            Title = other.Title;
            FileUrl = other.FileUrl;
            ExpiryDateTemplate = other.ExpiryDateTemplate;

            PublishDateTemplate = other.PublishDateTemplate;
            ExpiryAfterDownload = other.ExpiryAfterDownload;
            ExpiryAfterPlay = other.ExpiryAfterPlay;
            //   MaxDownloadAccount= maxDownloadAccount==null?null : (long?)long.Parse(maxDownloadAccount);
            ExpectedMD5 = other.ExpectedMD5;
            ExpectedSize = other.ExpectedSize;
            Metadata = other.Metadata;
            PermittedMimeTypes = other.PermittedMimeTypes;
            Quality = other.Quality;
            _mediaType = other._mediaType;
        }

        private int _mediaType;
        public EQualityLevel Quality { get; set; }
        public EAssetType AssetType { get { return _mediaType == 0 ? EAssetType.Single : EAssetType.Segmented; } }
        private long CalcQuality(long? max)
        {
            if (max.HasValue)
            {
                long m = max.Value;
                switch (Quality)
                {
                    case EQualityLevel.Low:
                        return 1;
                    case EQualityLevel.Medium:
                        if (m <= 0) return long.MaxValue;
                        return m / 2;
                    case EQualityLevel.High:
                        return m <= 0 ? long.MaxValue : m;
                }
            }
            return default(long);
        }

        public string AssetId { get; private set; }
        public string Title { get; private set; }
        public string FileUrl { get; private set; }
        public DateTime? PublishDate
        {
            get
            {
                if (PublishDateTemplate == null)
                    return null;

                var minusNowStr = PublishDateTemplate.Replace("{Now}", "");
                minusNowStr = minusNowStr.Replace("+", "");
                minusNowStr = minusNowStr.Replace("s", "");
                minusNowStr = minusNowStr.Replace("S", "");
                minusNowStr.Trim();
                double secsToAdd = double.Parse(minusNowStr);

                return DateTime.UtcNow.AddSeconds(secsToAdd);
            }
        }
        private string PublishDateTemplate { get; set; }
        public DateTime? ExpiryDate
        {
            get
            {
                if (ExpiryDateTemplate == null)
                    return null;

                var minusNowStr = ExpiryDateTemplate.Replace("{Now}", "");
                minusNowStr = minusNowStr.Replace("+", "");
                minusNowStr = minusNowStr.Replace("s", "");
                minusNowStr = minusNowStr.Replace("S", "");
                minusNowStr.Trim();
                double secsToAdd = double.Parse(minusNowStr);

                return DateTime.UtcNow.AddSeconds(secsToAdd);
            }
        }
        private string ExpiryDateTemplate { get; set; }
        public long? ExpiryAfterDownload { get; private set; }
        public long? ExpiryAfterPlay { get; private set; }
        public long? MaxDownloadAccount { get; private set; }
        public string ExpectedMD5 { get; private set; }
        public ulong? ExpectedSize { get; private set; }
        public List<string> PermittedMimeTypes { get; private set; }
        public string Metadata { get; private set; }

        public override string ToString()
        {
            return Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class QATest : INotifyPropertyChanged
    {
        public QATest() { }

        public QATest(String uniqueId, String title)
        {
            UniqueId = uniqueId;
            Title = title;
            Assets = new ObservableCollection<QATestAsset>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public ObservableCollection<QATestAsset> Assets { get; private set; }
        public bool IsSelected { get; set; }

        public QATest Clone()
        {
            var newTest = new QATest()
            {
                UniqueId = UniqueId,
                Title = Title
            };
            var newAssets = new ObservableCollection<QATestAsset>();
            foreach (var asset in Assets)
            {
                newAssets.Add(new QATestAsset(asset));
            }
            newTest.Assets = newAssets;
            return newTest;
        }

        //public bool IsCompleted
        //{
        //    get
        //    {
        //        if (_isCompleted) return true;
        //        // Check all the assets.
        //        if (Assets.Any(asset => asset.FractionComplete < 1.0)) return false;
        //        _isCompleted = true;
        //        OnPropertyChanged();
        //        OnDownloadComplete();
        //        return true;
        //    }
        //}
        //private bool _isCompleted;

        public override string ToString()
        {
            return Title;
        }

        public event DownloadCompleteEventHandler DownloadComplete;

        protected async virtual void OnDownloadComplete()
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var handler = DownloadComplete;
                if (handler != null) handler(this, null);
            });
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class QATestSection
    {
        public QATestSection(String uniqueId, String title)
        {
            UniqueId = uniqueId;
            Title = title;
            Tests = new ObservableCollection<QATest>();
        }

        public String UniqueId { get; private set; }
        public String Title { get; private set; }
        public ObservableCollection<QATest> Tests { get; private set; }

        public override string ToString()
        {
            return Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class QATestDataSource
    {
        private static readonly QATestDataSource _qaTestDataSource = new QATestDataSource();
        private readonly ObservableCollection<QATestSection> _sections = new ObservableCollection<QATestSection>();

        public ObservableCollection<QATestSection> Sections
        {
            get { return _sections; }
        }

        public static async Task<IEnumerable<QATestSection>> GetSectionsAsync()
        {
            if (_qaTestDataSource == null || _qaTestDataSource.Sections.Count == 0)
            {
                await _qaTestDataSource.GetSectionDataAsync();
            }
            return _qaTestDataSource.Sections;
        }

        public ObservableCollection<QATest> PendingTests
        {
            get
            {
                if (_pendingTests == null) _pendingTests = new ObservableCollection<QATest>();
                return _pendingTests;
            }
        }
        private ObservableCollection<QATest> _pendingTests = new ObservableCollection<QATest>();

        public static IEnumerable<QATest> GetPendingTests()
        {
            return _qaTestDataSource.PendingTests;
        }

        public ObservableCollection<IAsset> QueuedAssets
        {
            get
            {
                _queuedAssets = GetQueuedAssetsAsync().Result;
                return _queuedAssets;
            }
        }
        private ObservableCollection<IAsset> _queuedAssets;

        public async static Task<ObservableCollection<IAsset>> GetQueuedAssetsAsync()
        {
            ObservableCollection<IAsset> queue = null;
            await Windows.System.Threading.ThreadPool.RunAsync(
                (workitem) => queue = VirtuosoClientFactory.ClientInstance().ShowQueue());
            return queue;
        }

        public ObservableCollection<IAsset> PendingAssets
        {
            get
            {
                _pendingAssets = GetPendingAssetsAsync().Result;
                return _pendingAssets;
            }
        }
        private ObservableCollection<IAsset> _pendingAssets;

        public async static Task<ObservableCollection<IAsset>> GetPendingAssetsAsync()
        {
            App.VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "++ QATestDataSource.GetPendingAssetsAsync");
            ObservableCollection<IAsset> collection = null;
            await Windows.System.Threading.ThreadPool.RunAsync(
                (workitem) =>
                {
                    App.VClient.VirtuosoLogger.WriteLine(VirtuosoLoggingLevel.Debug, "++ QATestDataSource.get q");
                    collection = VirtuosoClientFactory.ClientInstance().Queue;
                }
                );
            return collection;
        }
        public ObservableCollection<IAsset> CompletedAssets
        {
            get
            {
                _completedAssets = GetCompletedAssetsAsync().Result;
                return _completedAssets;
            }
        }
        private ObservableCollection<IAsset> _completedAssets;

        public async static Task<ObservableCollection<IAsset>> GetCompletedAssetsAsync()
        {
            ObservableCollection<IAsset> collection = null;
            await Windows.System.Threading.ThreadPool.RunAsync(
                (workitem) => collection = VirtuosoClientFactory.ClientInstance().Downloaded);
            return collection;
        }

        public async static Task<IAsset> GetAssetAsync(Guid uuid)
        {
            IAsset asset = null;
            await Windows.System.Threading.ThreadPool.RunAsync(
                (workitem) => asset = VirtuosoClientFactory.ClientInstance().GetAsset(uuid));
            return asset;
        }

        public ObservableCollection<QATest> CompletedTests
        {
            get { return _completedTests ?? (_completedTests = new ObservableCollection<QATest>()); }
        }
        private ObservableCollection<QATest> _completedTests = new ObservableCollection<QATest>();

        public static IEnumerable<QATest> GetCompletedTests()
        {
            return _qaTestDataSource.CompletedTests;
        }

        public static async Task<QATestSection> GetSectionAsync(string uniqueId)
        {
            await _qaTestDataSource.GetSectionDataAsync();
            var matches = _qaTestDataSource.Sections.Where((section) => section.UniqueId.Equals(uniqueId));
            return matches.Count() == 1 ? matches.First() : null;
        }

        public static async Task<QATest> GetTestAsync(string uniqueId)
        {
            var matches = _qaTestDataSource.Sections.SelectMany(section => section.Tests)
                                                    .Where((test) => test.UniqueId.Equals(uniqueId));
            return matches.Count() == 1 ? matches.First() : null;
        }

        private async Task GetSectionDataAsync()
        {
            if (_sections.Count != 0) return;

            var dataUri = new Uri("ms-appx:///DataModel/QATestData.json");

            var file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            var jsonText = await FileIO.ReadTextAsync(file);
            var jsonObject = JsonObject.Parse(jsonText);
            var jsonArray = jsonObject["Sections"].GetArray();

            foreach (var sectionJsonValue in jsonArray)
            {
                var sectionObject = ((JsonValue)sectionJsonValue).GetObject();
                var section = new QATestSection(sectionObject["UniqueId"].GetString(), sectionObject["Title"].GetString());

                foreach (var jsonValue in sectionObject["Tests"].GetArray())
                {
                    var testObject = ((JsonValue)jsonValue).GetObject();
                    var test = new QATest(testObject["UniqueId"].GetString(), testObject["Title"].GetString());

                    foreach (var jsonValue1 in testObject["Assets"].GetArray())
                    {
                        var assetValue = (JsonValue)jsonValue1;
                        var assetObject = (JsonObject)assetValue.GetObject();
                        test.Assets.Add(new QATestAsset(
                            GetString(assetObject["AssetId"]),
                            GetString(assetObject["Title"]),
                            GetString(assetObject["FileUrl"]),
                            GetString(assetObject["PublishDate"]),
                            GetString(assetObject["ExpiryDate"]),
                            GetString(assetObject["ExpiryAfterDownload"]),
                            GetString(assetObject["ExpiryAfterPlay"]),
                            GetString(assetObject, "MD5", null),
                            GetString(assetObject, "Size", null),
                            GetString(assetObject["MimeTypes"]),
                            GetString(assetObject["Metadata"]),
                            GetString(assetObject, "mediaType", "0")));
                    }
                    section.Tests.Add(test);
                }
                Sections.Add(section);
            }
        }
        private string GetString(JsonObject value, string propertyName, string defaultValue)
        {
            string ret = null;
            if (value.ContainsKey(propertyName))
            {
                ret = GetString(value[propertyName]);
            }

            return ret == null ? defaultValue : ret;

        }

        private String GetString(IJsonValue value)
        {
            if (value.ValueType == JsonValueType.Null) return null;
            if (value.ValueType == JsonValueType.String) return value.GetString();
            if (value.ValueType == JsonValueType.Number) return value.GetNumber().ToString();
            return value.ToString();
        }

    }
}
