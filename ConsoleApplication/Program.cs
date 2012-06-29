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
            string email = "josefwurzel1980@googlemail.com";
            string password = "";
            string baseDirPath = String.IsNullOrEmpty(ConfigurationManager.AppSettings["BasePath"]) ? @"c:\temp\" : ConfigurationManager.AppSettings["BasePath"];
            int itemsToGetPerFeed = int.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["ItemsPerFeed"]) ? "10" : ConfigurationManager.AppSettings["ItemsPerFeed"]);

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Enter password");
                password = Console.ReadLine();
            }

            Console.Clear();

            using (Reader reader = Reader.CreateReader(email, password, "scroll") as Reader)
            {
                Console.WriteLine("Getting...");

                var listenSubscriptions = "Listen Subscriptions";

                int podCastsdownloaded = 0;

                foreach (var urlAndFeed in GetFeedsWithGivenLabel.Process(reader, listenSubscriptions))
                {
                    // fork
                    var dirPath = EnsureDownloadDirectoryForFeed.Process(urlAndFeed.Feed, baseDirPath);

                    // join...
                    foreach (var podcastLink in GetPodcastLinksFromFeed.Process(reader, urlAndFeed.Url, itemsToGetPerFeed))
                    {
                        var remoteAndLocalAddress = GetRemoteAndLocalAddress.Process(dirPath, podcastLink);

                        if (File.Exists(remoteAndLocalAddress.LocalAddress))
                            continue;

                        DownloadFile.Process(remoteAndLocalAddress);

                        podCastsdownloaded++;

                        Console.Clear();
                    }
                }
                Console.WriteLine("Podcasts downloaded: {0}", podCastsdownloaded);
            }
        }
    }
}
