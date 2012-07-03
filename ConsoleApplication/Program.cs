using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using GoogleReaderAPI2;
using Tools;
using Tools.DnpExtensions;
using npantarhei.runtime.patterns.operations;

namespace ConsoleApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string email = ConfigurationManager.AppSettings["GoogleAccount"];
            string baseDirPath = String.IsNullOrEmpty(ConfigurationManager.AppSettings["BasePath"]) ? @"c:\temp\" : ConfigurationManager.AppSettings["BasePath"];
            int itemsToGetPerFeed = int.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["ItemsPerFeed"]) ? "10" : ConfigurationManager.AppSettings["ItemsPerFeed"]);
            var listenSubscriptions = "Listen Subscriptions";
            int podCastsdownloaded = 0;
            string dateFormat = "yyyyMMddTHHmmss";
            int deleteFilesOlderThanXDays = int.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["KeepFilesForXDays"]) ? "7" : ConfigurationManager.AppSettings["KeepFilesForXDays"]);
            Reader reader = null;

            Console.WriteLine("Enter password");
            var password = Console.ReadLine();

            reader = Reader.CreateReader(email, password, "scroll") as Reader;

            GetFeedsWithGivenLabel getFeedsWithGivenLabel = new GetFeedsWithGivenLabel(listenSubscriptions);
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
            DownloadFile downloadFile1 = new DownloadFile();
            DownloadFile downloadFile2 = new DownloadFile();

            Scatter<RemoteAndLocalAddress> scatter = new Scatter<RemoteAndLocalAddress>();

            getFeedsWithGivenLabel.Result += urlAndFeed => ensureDownloadDirectoryForFeed.Process(urlAndFeed.Feed);
            getFeedsWithGivenLabel.Result += urlAndFeed => getPodcastLinksFromFeed.Process(urlAndFeed.Url);
            //getFeedsWithGivenLabel.OnFeedFound += Console.Clear;
            ensureDownloadDirectoryForFeed.Result += clearDirectoryOfFilesOlderThan.Process;
            clearDirectoryOfFilesOlderThan.Result += getRemoteAndLocalAddress.ProcessDirPath;
            getPodcastLinksFromFeed.Result += getRemoteAndLocalAddress.ProcessPodcastLinkInformation;
            getRemoteAndLocalAddress.Result += filterExistingFiles.Process;
            filterExistingFiles.ResultForNotExistingFile += scatter.Process;
            scatter.Output1 += downloadFile1.Process;
            scatter.Output2 += downloadFile2.Process;
            filterExistingFiles.ResultForNotExistingFile += _ => podCastsdownloaded += _.Count();
            //filterExistingFiles.ResultForExistingFile += _ => Console.Clear();

            getFeedsWithGivenLabel.Process(reader);
            
            Console.WriteLine("Podcasts downloaded: {0}", podCastsdownloaded);

            Console.ReadLine();
        }
    }
}
