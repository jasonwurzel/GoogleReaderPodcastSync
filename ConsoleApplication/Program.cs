using System;
using System.Collections.Generic;
using System.Configuration;
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
        static void Main(string[] args)
        {
            //  TODO: enter your google account information here
            string email = "josefwurzel1980@googlemail.com";
            string password = "";
            string baseDirPath = String.IsNullOrEmpty(ConfigurationManager.AppSettings["BasePath"]) ? @"c:\temp\" : ConfigurationManager.AppSettings["BasePath"];

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Enter password");
                password = Console.ReadLine();
            }

            using (Reader reader = Reader.CreateReader(email, password, "scroll") as Reader)
            {
                Console.WriteLine("Getting...");
                
                var listenSubscriptions = "Listen Subscriptions";


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
                        foreach (var item in reader.GetFeed(unreadFeed.Url, 20).Items)
                        {
                            Console.WriteLine(item.PublishDate);
                            Console.WriteLine(item.Title.Text);
                            Console.WriteLine("File:");
                            var links = item.Links.Where(l => l.RelationshipType.ToLower() == "enclosure");
                            foreach (var syndicationLink in links)
                            {
                                string remoteAdress = syndicationLink.Uri.OriginalString;
                                Console.WriteLine(syndicationLink.Uri.OriginalString);
                                string localFileName = item.PublishDate.ToString("yyyyMMddTHHmmss") + "_" + item.Title.Text.ToValidFileName() + ".mp3";
                                string localFilePath = Path.Combine(dirPath, localFileName);
                                Console.WriteLine(localFileName);
                                
                                if (File.Exists(localFilePath))
                                    continue;

                                DownloadFile.Process(localFilePath, remoteAdress);
                            }
                        }
                    }
                }
            }

        }
    }


}
