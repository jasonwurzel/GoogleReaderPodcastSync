using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Flows.DownloadPodcastsFromReaderFlows;
using GoogleReaderAPI2;
using NUnit.Framework;
using Repository;
using Tools;
using npantarhei.runtime;
using npantarhei.runtime.contract;
using npantarhei.runtime.messagetypes;
using npantarhei.runtime.patterns;
using npantarhei.runtime.patterns.operations;

namespace UnitTests
{
    [TestFixture]
    public class UnitTest2
    {
        private IFeedRepository reader;

        [Test]
        public void TestMethod1()
        {
            string email = "josefwurzel1980@googlemail.com";
            string password = "";
            reader = new FeedRepository(Reader.CreateReader(email, password, "scroll") as Reader);
            //reader = new FeedRepositoryForTests();

            var frc = new FlowRuntimeConfiguration();
            frc.AddStreamsFrom(@"
                                .in, getFeeds
                                getFeeds.out0, getLinks
                                getFeeds.out1, join.in0
                                getLinks, getAddress
                                getAddress, scatter
                                scatter.count, gather.count
                                scatter.stream, downloadFile
                                downloadFile, gather.stream
                                gather, join.in1
                                join, .out
                                ");

            frc.AddAction<string, UrlAndFeed, int>("getFeeds", getFeeds);
            frc.AddFunc<int, bool>("feedCount", i => { return true; });
            frc.AddFunc<UrlAndFeed, IEnumerable<PodcastLinkInformation>>("getLinks", urlAndFeed => reader.GetItemsForFeed(urlAndFeed.Url, DateTime.Now.AddDays(-1)));
            frc.AddFunc<IEnumerable<PodcastLinkInformation>, IEnumerable<RemoteAndLocalAddress>>("getAddress", getAddress);
            frc.AddFunc<RemoteAndLocalAddress, RemoteAndLocalAddress>("downloadFile", downloadFile).MakeParallel();
            frc.AddOperation(new Scatter<RemoteAndLocalAddress>("scatter"));
            frc.AddOperation(new Gather<RemoteAndLocalAddress>("gather"));
            frc.AddOperation(new MySimpleJoin());

            using (var fr = new FlowRuntime(frc))
            {
                fr.Process(".in", "Listen Subscriptions");

                object erg;
                fr.WaitForResult(message => erg = message.Data);
                //Thread.Sleep(60000);
            }
        }

        private void getFeeds(string s, Action<UrlAndFeed> continueWith, Action<int> getCount)
        {
            IEnumerable<UrlAndFeed> urlAndFeeds = reader.GetFeeds(s);
            getCount(urlAndFeeds.Count());
            foreach (var urlAndFeed in urlAndFeeds) { continueWith(urlAndFeed); }
        }

        private RemoteAndLocalAddress downloadFile(RemoteAndLocalAddress remoteAndLocalAddress)
        {
            try
            {
                using (var webClient = new WebClient())
                    webClient.DownloadFile(remoteAndLocalAddress.RemoteAddress, remoteAndLocalAddress.LocalAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return remoteAndLocalAddress;
        }

        private IEnumerable<RemoteAndLocalAddress> getAddress(IEnumerable<PodcastLinkInformation> podcastLinkInformations)
        {
            var list = new List<RemoteAndLocalAddress>();
            foreach (var link in podcastLinkInformations)
            {
                string remoteAddress = link.FileAddress;
                string localFileName = link.PublishDate.ToString("yyyyMMddTHHmmss") + "_" + link.Title.ToValidFileName() + ".mp3";
                string dirPath = @"c:/temp";
                string localFilePath = Path.Combine(dirPath, localFileName);
                list.Add(new RemoteAndLocalAddress(remoteAddress, localFilePath));
            }

            return list;
        }
    }

    public class MySimpleJoin : AOperation
    {
        private int _count0;
        private int _count1;
        public MySimpleJoin() : this("join") { }
        public MySimpleJoin(string name) : base(name) { }

        protected override void Process(IMessage input, Action<IMessage> continueWith, Action<FlowRuntimeException> unhandledException)
        {
            switch (input.Port.Name.ToLower())
            {
                case "in0":
                    _count0 += (int) input.Data;
                    checkConditions(continueWith);
                    break;
                case "in1":
                    _count1++;
                    checkConditions(continueWith);
                    break;
                default:
                    throw new ArgumentException("Input port not supported by MyJoin: " + input.Port.Name);
            }
        }

        private void checkConditions(Action<IMessage> continueWith)
        {
            if (_count0 == _count1)
            {
                continueWith(new Message(base.Name, _count0));
            }
        }
    }

    public class FeedRepositoryForTests : IFeedRepository
    {
        public IEnumerable<UrlAndFeed> GetFeeds(string label)
        {

            var list = new List<UrlAndFeed>()
                           {
                               new UrlAndFeed(@"http://www.dradio.de/rss/podcast/sendungen/drw_medien?hl=de", "medien"),
                               new UrlAndFeed(@"http://wissen.dradio.de/spielraum.8.29.de.podcast?hl=de", "spielraum")
                           };
            return list;
        }

        public IEnumerable<PodcastLinkInformation> GetItemsForFeed(string feedUrl, DateTime earliestPublishDate)
        {
            var list = new List<PodcastLinkInformation>()
                           {
                               new PodcastLinkInformation(
                                   @"http://ondemand-mp3.dradio.de/file/dradio/2012/08/06/drw_201208060834_us-radios_vor_dem_aus_fcd93f47.mp3",
                                   new DateTimeOffset(2012, 08, 06, 12, 00, 00, new TimeSpan(2, 0, 0)),
                                   "USA - Public Radio in Gefahr")
                           };

            return list;
        }
    }
}
