using System;
using System.Collections.Generic;
using System.Configuration;
using ConsoleApplication;
using GoogleReaderAPI2;
using npantarhei.runtime;
using npantarhei.runtime.messagetypes;

namespace ConsoleApplicationNPR
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
            //ClearDirectoryOfFiles clearDirectoryOfFilesOlderThan = new ClearDirectoryOfFiles(dirPath =>
            //{
            //    var list = new List<string>();
            //    foreach (var filePath in Directory.GetFiles(dirPath))
            //    {
            //        var file = Path.GetFileName(filePath);
            //        var dateString = file.Substring(0, dateFormat.Length);
            //        DateTime dt = DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);
            //        if (dt < DateTime.Now.AddDays(-deleteFilesOlderThanXDays))
            //            list.Add(filePath);
            //    }
            //    return list;
            //});
            //GetPodcastLinksFromFeed getPodcastLinksFromFeed = new GetPodcastLinksFromFeed(reader, itemsToGetPerFeed);
            //GetRemoteAndLocalAddress getRemoteAndLocalAddress = new GetRemoteAndLocalAddress(link => link.PublishDate.ToString(dateFormat) + "_" + link.Title.ToValidFileName() + ".mp3");
            //FilterExistingFiles filterExistingFiles = new FilterExistingFiles();
            //DownloadFile downloadFile = new DownloadFile();

            using (var fr = new FlowRuntime())
            {
                var frc = new FlowRuntimeConfiguration();
                frc.AddStream(".in", "Get_Feeds");
                frc.AddStream("Get_Feeds", "Ensure_Download_Directory");
                frc.AddStream("Ensure_Download_Directory", ".out");
                //frc.AddStream("Get_Feeds", "Get_Podcast_Links");
                //frc.AddStream("Ensure_Download_Directory", "Tidy_Up_Directory");
                //frc.AddStream("Get_Podcast_Links", "Remote_And_Local_Adress.Process_PodcastLinkInformation");
                //frc.AddStream("Tidy_Up_Directory", "Remote_And_Local_Adress.Process_Local_Path");
                //frc.AddStream("Remote_And_Local_Adress", "Filter_Existing_Files");
                //frc.AddStream("Filter_Existing_Files.NonExistentFile", "DownloadFile");
                //frc.AddStream("Filter_Existing_Files.NonExistentFile", "Increment_Download_Counter");
                //frc.AddStream("Filter_Existing_Files.ExistingFile", "Clear");

                frc.AddFunc<Reader, IEnumerable<UrlAndFeed>>("Get_Feeds", getFeedsWithGivenLabel.ProcessNPR);
                frc.AddFunc<UrlAndFeed, string>("Ensure_Download_Directory", urlAndFeed => ensureDownloadDirectoryForFeed.ProcessNPR(urlAndFeed.Feed));

                fr.Configure(frc);

                fr.Process(new Message(".in", reader));

                fr.WaitForResult(5000, _ => Console.WriteLine(_.Data));

                Console.ReadLine();
            }

            //getFeedsWithGivenLabel.Result += urlAndFeed => ensureDownloadDirectoryForFeed.Process(urlAndFeed.Feed);
            //getFeedsWithGivenLabel.Result += urlAndFeed => getPodcastLinksFromFeed.Process(urlAndFeed.Url);
            //getFeedsWithGivenLabel.OnFeedFound += Console.Clear;
            //ensureDownloadDirectoryForFeed.Result += clearDirectoryOfFilesOlderThan.Process;
            //clearDirectoryOfFilesOlderThan.Result += getRemoteAndLocalAddress.ProcessDirPath;
            //getPodcastLinksFromFeed.Result += getRemoteAndLocalAddress.ProcessPodcastLinkInformation;
            //getRemoteAndLocalAddress.Result += filterExistingFiles.Process;
            //filterExistingFiles.ResultForNotExistingFile += downloadFile.Process;
            //filterExistingFiles.ResultForNotExistingFile += _ => podCastsdownloaded++;
            //filterExistingFiles.ResultForExistingFile += _ => Console.Clear();

            //getFeedsWithGivenLabel.Process(reader);

            //Console.WriteLine("Podcasts downloaded: {0}", podCastsdownloaded);

            //Console.ReadLine();
        }
    }
}
