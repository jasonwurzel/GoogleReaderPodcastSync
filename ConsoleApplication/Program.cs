using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using GoogleReaderAPI2;
using Tools;
using Tools.DnpExtensions;

namespace ConsoleApplication
{
    class Program
    {

        static void Main(string[] args)
        {
            string email = ConfigurationManager.AppSettings["GoogleAccount"];
            string baseDirPath = String.IsNullOrEmpty(ConfigurationManager.AppSettings["BasePath"]) ? @"c:\temp\" : ConfigurationManager.AppSettings["BasePath"];
            int itemsToGetPerFeed = int.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["ItemsPerFeed"]) ? "10" : ConfigurationManager.AppSettings["ItemsPerFeed"]);
            var listenSubscriptions = "Listen Subscriptions";
            int podCastsdownloaded = 0;
            string password = "";
            string dateFormat = "yyyyMMddTHHmmss";
            string dateRegex = "3";
            int deleteFilesOlderThanXDays = 2;


            ReadPasswordFromConsole readPasswordFromConsole = new ReadPasswordFromConsole();
            readPasswordFromConsole.Result += s => password = s;
            readPasswordFromConsole.Process();

            Console.Clear();

            // Until i come up with a solution how to deal with IDisposable, the flow is interrupted here...
            using (Reader reader = Reader.CreateReader(email, password, "scroll") as Reader)
            {
                GetFeedsWithGivenLabel getFeedsWithGivenLabel = new GetFeedsWithGivenLabel(reader);
                EnsureDownloadDirectoryForFeed ensureDownloadDirectoryForFeed = new EnsureDownloadDirectoryForFeed(baseDirPath);
                ClearDirectoryOfFiles clearDirectoryOfFilesOlderThan = new ClearDirectoryOfFiles(dirPath =>
                                                                                            {
                                                                                                var list = new List<string>();
                                                                                                foreach (var filePath in Directory.GetFiles(dirPath))
                                                                                                {
                                                                                                    var file = Path.GetFileName(filePath);
                                                                                                    var dateString = file.Substring(0, dateFormat.Length);
                                                                                                    DateTime dt = DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);
                                                                                                    if (dt < DateTime.Now.AddDays(-deleteFilesOlderThanXDays))
                                                                                                        list.Add(filePath);
                                                                                                }
                                                                                                return list;
                                                                                            });
                GetPodcastLinksFromFeed getPodcastLinksFromFeed = new GetPodcastLinksFromFeed(reader, itemsToGetPerFeed);
                GetRemoteAndLocalAddress getRemoteAndLocalAddress = new GetRemoteAndLocalAddress(link => link.PublishDate.ToString(dateFormat) + "_" + link.Title.ToValidFileName() + ".mp3");
                FilterExistingFiles filterExistingFiles = new FilterExistingFiles();
                DownloadFile downloadFile = new DownloadFile();

                getFeedsWithGivenLabel.Result += urlAndFeed => ensureDownloadDirectoryForFeed.Process(urlAndFeed.Feed);
                getFeedsWithGivenLabel.Result += urlAndFeed => getPodcastLinksFromFeed.Process(urlAndFeed.Url);
                getFeedsWithGivenLabel.OnFeedFound += Console.Clear;
                ensureDownloadDirectoryForFeed.Result += clearDirectoryOfFilesOlderThan.Process;
                clearDirectoryOfFilesOlderThan.Result += getRemoteAndLocalAddress.ProcessDirPath;
                getPodcastLinksFromFeed.Result += getRemoteAndLocalAddress.ProcessPodcastLinkInformation;
                getRemoteAndLocalAddress.Result += filterExistingFiles.Process;
                filterExistingFiles.ResultForNotExistingFile += downloadFile.Process;
                filterExistingFiles.ResultForNotExistingFile += _ => podCastsdownloaded++;
                filterExistingFiles.ResultForExistingFile += _ => Console.Clear();

                getFeedsWithGivenLabel.Process(listenSubscriptions);
                
                Console.WriteLine("Podcasts downloaded: {0}", podCastsdownloaded);

                Console.ReadLine();
            }
        }
    }

    public class ClearDirectoryOfFiles
    {
        private Func<string, IEnumerable<string>> _findFiles;

        public ClearDirectoryOfFiles(Func<string, IEnumerable<string>> findFilesToDelete)
        {
            _findFiles = findFilesToDelete;
        }
        public void Process(string dirPath)
        {
            List<string> files = new List<string>();

            foreach (var file in _findFiles(dirPath).ToList())
            {
                File.Delete(file);
            }

            // einfach durchreichen
            Result(dirPath);
        }

        public event Action<string> Result;
    }

    public class FilterExistingFiles
    {
        public void Process(RemoteAndLocalAddress address)
        {
            if (!File.Exists(address.LocalAddress))
                ResultForNotExistingFile(address);
            else
                ResultForExistingFile(address);
        }

        public event Action<RemoteAndLocalAddress> ResultForNotExistingFile;
        public event Action<RemoteAndLocalAddress> ResultForExistingFile;
    }
}
