using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using GoogleReaderAPI2;
using Tools;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //  TODO: enter your google account information here
            string email = ConfigurationManager.AppSettings["GoogleAccount"];
            string baseDirPath = String.IsNullOrEmpty(ConfigurationManager.AppSettings["BasePath"]) ? @"c:\temp\" : ConfigurationManager.AppSettings["BasePath"];
            int itemsToGetPerFeed = int.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["ItemsPerFeed"]) ? "10" : ConfigurationManager.AppSettings["ItemsPerFeed"]);
            var listenSubscriptions = "Listen Subscriptions";
            int podCastsdownloaded = 0;
            string password = "";

            ReadPasswordFromConsole readPasswordFromConsole = new ReadPasswordFromConsole();
            readPasswordFromConsole.Result += s => password = s;
            readPasswordFromConsole.Process();

            Console.Clear();

            // Until i came up with a solution how to deal with IDisposable, the flow is interrupted here...
            using (Reader reader = Reader.CreateReader(email, password, "scroll") as Reader)
            {

                GetFeedsWithGivenLabel getFeedsWithGivenLabel = new GetFeedsWithGivenLabel(reader);
                EnsureDownloadDirectoryForFeed ensureDownloadDirectoryForFeed = new EnsureDownloadDirectoryForFeed(baseDirPath);
                GetPodcastLinksFromFeed getPodcastLinksFromFeed = new GetPodcastLinksFromFeed(reader, itemsToGetPerFeed);
                GetRemoteAndLocalAddress getRemoteAndLocalAddress = new GetRemoteAndLocalAddress();
                FilterExistingFiles filterExistingFiles = new FilterExistingFiles();
                DownloadFile downloadFile = new DownloadFile();

                getFeedsWithGivenLabel.Result += urlAndFeed => ensureDownloadDirectoryForFeed.Process(urlAndFeed.Feed);
                getFeedsWithGivenLabel.Result += urlAndFeed => getPodcastLinksFromFeed.Process(urlAndFeed.Url);
                ensureDownloadDirectoryForFeed.Result += getRemoteAndLocalAddress.ProcessDirPath;
                getPodcastLinksFromFeed.Result += getRemoteAndLocalAddress.ProcessPodcastLinkInformation;
                getRemoteAndLocalAddress.Result += filterExistingFiles.Process;
                filterExistingFiles.Result += downloadFile.Process;
                filterExistingFiles.Result += _ => podCastsdownloaded++;

                getFeedsWithGivenLabel.Process(listenSubscriptions);
                
                Console.WriteLine("Podcasts downloaded: {0}", podCastsdownloaded);

                Console.ReadLine();
            }
        }
    }

    public class FilterExistingFiles
    {
        public void Process(RemoteAndLocalAddress address)
        {
            if (!File.Exists(address.LocalAddress))
                Result(address);
        }

        public event Action<RemoteAndLocalAddress> Result;
    }
}
