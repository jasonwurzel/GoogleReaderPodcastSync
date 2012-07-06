using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
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
            ClearDirectoryOfFiles clearDirectoryOfFilesOlderThan = new ClearDirectoryOfFiles(dateFormat, deleteFilesOlderThanXDays);
            GetPodcastLinksFromFeed getPodcastLinksFromFeed = new GetPodcastLinksFromFeed(reader, itemsToGetPerFeed);
            GetRemoteAndLocalAddress getRemoteAndLocalAddress = new GetRemoteAndLocalAddress(link => link.PublishDate.ToString(dateFormat) + "_" + link.Title.ToValidFileName() + ".mp3");
            FilterExistingFiles filterExistingFiles = new FilterExistingFiles();
            DownloadFile downloadFile1 = new DownloadFile();
            Counter counter = new Counter();

            //ScatterStream<RemoteAndLocalAddress> scatterStream = new ScatterStream<RemoteAndLocalAddress>();

            getFeedsWithGivenLabel.Result += _ => counter.CountOneFeed();
            getFeedsWithGivenLabel.Result += urlAndFeed => ensureDownloadDirectoryForFeed.Process(urlAndFeed.Feed);
            getFeedsWithGivenLabel.Result += urlAndFeed => getPodcastLinksFromFeed.Process(urlAndFeed.Url);
            
            ensureDownloadDirectoryForFeed.Result += clearDirectoryOfFilesOlderThan.Process;
            clearDirectoryOfFilesOlderThan.Result += getRemoteAndLocalAddress.ProcessDirPath;
            getPodcastLinksFromFeed.Result += getRemoteAndLocalAddress.ProcessPodcastLinkInformation;
            getPodcastLinksFromFeed.SignalTotalCount += counter.SetItemCountToExpect;
            getFeedsWithGivenLabel.SignalTotalCount += counter.SetFeedCountToExpect;
            getRemoteAndLocalAddress.Result += filterExistingFiles.Process;
            filterExistingFiles.ResultForNotExistingFile += downloadFile1.Process;
            downloadFile1.Result += _ => counter.CountOneItem();
            
            getFeedsWithGivenLabel.Process(reader);


            Console.WriteLine("************* Fertig **************");
            Console.ReadLine();
        }
    }
}
