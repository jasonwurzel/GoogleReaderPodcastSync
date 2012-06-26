using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace GoogleReaderAPI
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            //  TODO: enter your google account information here
            string email = "josefwurzel1980@googlemail.com";
            string password = "sapchikauda";

            

            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Enter password");
                password = Console.ReadLine();
            }

            using (Reader reader = Reader.CreateReader(email, password, "scroll") as Reader)
            {
                Console.WriteLine("Getting...");
                // test any reader command here:
                var unreadCount = reader.GetUnreadCount();

                //foreach (var unreadFeed in reader.GetItemsByLabel("Listen Subscriptions"))
                //{
                //    Console.WriteLine(unreadFeed.Title);
                //    Console.WriteLine(unreadFeed.Enclosure);
                //}
                var listenSubscriptions = "Listen Subscriptions";
                string baseDirPath = @"c:\temp\";


                foreach (var unreadFeed in reader.GetUnreadFeeds())
                {
                    // Leider Umweg nötig
                    var syndicationFeed = reader.GetFeed(unreadFeed.Url, 1);

                    if (syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == listenSubscriptions)))
                    {
                        string dirPath = Path.Combine(baseDirPath, ToValidDirName(syndicationFeed.Title.Text));
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);

                        Console.WriteLine(unreadFeed.Url);
                        foreach (var item in reader.GetFeed(unreadFeed.Url, 1).Items)
                        {
                            Console.WriteLine(item.PublishDate);
                            Console.WriteLine(item.Title.Text);
                            Console.WriteLine("File:");
                            var links = item.Links.Where(l => l.RelationshipType.ToLower() == "enclosure");
                            foreach (var syndicationLink in links)
                            {
                                Console.WriteLine(syndicationLink.Uri.OriginalString);
                                string localFileName = item.PublishDate.ToString("yyyyMMddTHHmmss") + "_" + ToValidFileName(item.Title.Text) + ".mp3";
                                string localFilePath = Path.Combine(dirPath, localFileName);
                                Console.WriteLine(localFileName);

                                using(WebClient webClient = new WebClient())
                                    webClient.DownloadFile(syndicationLink.Uri.OriginalString, localFilePath);

                            }
                        }
                    }
                }
            }
        }

        private static string ToValidFileName(string s)
        {
            return RemoveAll(HttpUtility.HtmlDecode(s), Path.GetInvalidFileNameChars()).Replace("\"", "");
        }

        private static string ToValidDirName(string s)
        {
            return RemoveAll(HttpUtility.HtmlDecode(s), Path.GetInvalidPathChars()).Replace("\"", "");
        }

        private static string RemoveAll(string s, char[] toRemove)
        {
            string erg = "";
            foreach (var c in toRemove)
                erg = s.Replace(c.ToString(), String.Empty);

            return erg;
        }

        public void Test01()
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(@"http://podcast-mp3.dradio.de/podcast/2012/03/10/dlf_20120310_1630_de6b40ac.mp3", @"c:\temp\test.mp3");
        }
    }


}