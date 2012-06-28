using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using GoogleReaderAPI2;
using Tools;

namespace ConsoleApplication
{
    class Program
    {
        private static bool _downloadrunning;

        static void Main(string[] args)
        {
            //  TODO: enter your google account information here
            string email = "josefwurzel1980@googlemail.com";
            string password = "";

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Enter password");
                password = Console.ReadLine();
            }

            using (Reader reader = Reader.CreateReader(email, password, "scroll") as Reader)
            {
                Console.WriteLine("Getting...");
                
                var listenSubscriptions = "Listen Subscriptions";
                string baseDirPath = @"c:\temp\";


                foreach (var unreadFeed in reader.GetUnreadFeeds())
                {
                    // Leider Umweg nötig
                    var syndicationFeed = reader.GetFeed(unreadFeed.Url, 1);

                    if (syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == listenSubscriptions)))
                    {
                        string dirPath = Path.Combine(baseDirPath, syndicationFeed.Title.Text.ToValidDirName());
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);

                        Console.WriteLine(unreadFeed.Url);
                        foreach (var item in reader.GetFeed(unreadFeed.Url, 21).Items)
                        {
                            Console.WriteLine(item.PublishDate);
                            Console.WriteLine(item.Title.Text);
                            Console.WriteLine("File:");
                            var links = item.Links.Where(l => l.RelationshipType.ToLower() == "enclosure");
                            foreach (var syndicationLink in links)
                            {
                                Console.WriteLine(syndicationLink.Uri.OriginalString);
                                string localFileName = item.PublishDate.ToString("yyyyMMddTHHmmss") + "_" + item.Title.Text.ToValidFileName() + ".mp3";
                                string localFilePath = Path.Combine(dirPath, localFileName);
                                Console.WriteLine(localFileName);
                                
                                if (File.Exists(localFilePath))
                                    continue;

                                try
                                {
                                    using (WebClient webClient = new WebClient())
                                    {
                                        _downloadrunning = true;
                                        webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
                                        webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;
                                        webClient.DownloadFileAsync(new Uri(syndicationLink.Uri.OriginalString), localFilePath);
                                        while (_downloadrunning)
                                            Thread.Sleep(500);
                                    }
                                }
                                finally
                                {
                                    _downloadrunning = false;
                                }
                            }
                        }
                    }
                }
            }

        }

        static void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine();
            _downloadrunning = false;
        }

        static void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write("\x000D                                  ");
            Console.Write("\x000DReceived {0} percent", (int)(100.0 * e.BytesReceived / e.TotalBytesToReceive));
        }
    }


}
