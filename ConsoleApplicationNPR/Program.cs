using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Syndication;
using GoogleReaderAPI2;
using Repository;
using npantarhei.runtime;
using npantarhei.runtime.messagetypes;

namespace ConsoleApplicationNPR
{
    internal class Program
    {
        private static string _label;
        private static string _email;
        private static string _baseDirPath;
        private static bool _deleteOlderFiles;
        private static string _dateFormat;
        private static int _getFilesFromTheLastXDays;
        private static Reader _reader;

        private static void Main(string[] args)
        {
            _email = ConfigurationManager.AppSettings["GoogleAccount"];
            _baseDirPath = String.IsNullOrEmpty(ConfigurationManager.AppSettings["BasePath"]) ? @"c:\temp\" : ConfigurationManager.AppSettings["BasePath"];
            _deleteOlderFiles = bool.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["DeleteOlderFiles"]) ? "false" : ConfigurationManager.AppSettings["DeleteOlderFiles"]);
            _label = "Listen Subscriptions";
            _dateFormat = "yyyyMMddTHHmmss";
            _getFilesFromTheLastXDays =
                int.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["GetFilesFromTheLastXDays"]) ? "3" : ConfigurationManager.AppSettings["GetFilesFromTheLastXDays"]);
            _reader = null;

            //Console.WriteLine("Enter password");
            var password = "sapchikauda";//  Console.ReadLine();

            _reader = Reader.CreateReader(_email, password, "scroll") as Reader;

            using (var fr = new FlowRuntime())
            {
                var frc = new FlowRuntimeConfiguration();
                frc.AddStream(".in", "Get_Feeds");
                frc.AddStream("Get_Feeds", ".out");

                frc.AddFunc<Reader, IEnumerable<UrlAndFeed>>("Get_Feeds", getFeedsWithGivenLabel);

                fr.Configure(frc);

                fr.Process(new Message(".in", _reader));

                fr.WaitForResult(5000, _ => Console.WriteLine(_.Data));

                Console.ReadLine();
            }
        }

        private static IEnumerable<UrlAndFeed> getFeedsWithGivenLabel(Reader reader)
        {
            var unreadFeeds = reader.GetUnreadFeeds();

            List<SyndicationFeed> feeds =
                unreadFeeds.
                Select(unreadFeed => reader.GetFeed(unreadFeed.Url, 1)).
                Where(syndicationFeed => syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == _label))).
                ToList();
            
            foreach (var syndicationFeed in feeds)
            {
                string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();

                yield return new UrlAndFeed(url, syndicationFeed.Title.Text);
            }
        }
    }
}
