using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using GoogleReaderAPI2;
using Tools.DnpExtensions;

namespace Flows.Npr.DownloadPodcastsFromReaderFlows
{
    public class GetPodcastLinksFromFeed
    {
        private Reader _reader;
        private int _getFilesFromTheLastXDays;

        public GetPodcastLinksFromFeed(Reader reader, int getFilesFromTheLastXDays)
        {
            _reader = reader;
            _getFilesFromTheLastXDays = getFilesFromTheLastXDays;
        }

        public void Process(string feedUrl)
        {
            var listOfLinks = new List<PodcastLinkInformation>();
            // Den ganzen Tag betrachten!
            var now = DateTime.Now;
            var dateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime minimumPublishDate = dateTime.AddDays(-_getFilesFromTheLastXDays);
            DateTimeOffset actualEarliestPublishDate = dateTime;
            int itemsToGetPerFeed = 30;

            IEnumerable<SyndicationItem> syndicationItems;
            syndicationItems = _reader.GetFeed(feedUrl, itemsToGetPerFeed).Items.OrderBy(item => item.PublishDate);
            actualEarliestPublishDate = syndicationItems.First().PublishDate;
            while (actualEarliestPublishDate > minimumPublishDate)
            {
                itemsToGetPerFeed += 10;
                syndicationItems = _reader.GetFeed(feedUrl, itemsToGetPerFeed).Items.OrderBy(item => item.PublishDate);
                actualEarliestPublishDate = syndicationItems.First().PublishDate;
            }

            if (SignalTotalCount != null)
                SignalTotalCount(syndicationItems.Count());

            foreach (var item in syndicationItems.Where(item => item.PublishDate > minimumPublishDate))
            {
                //Console.WriteLine(item.PublishDate);
                //Console.WriteLine(item.Title.Text);
                var links = item.Links.Where(l => l.RelationshipType.ToLower() == "enclosure");
                SyndicationItem item1 = item;
                listOfLinks.AddRange(EnumerableExtensions.Select(links, l => new PodcastLinkInformation(l.Uri.OriginalString, item1.PublishDate, item1.Title.Text)));
            }

            Result(listOfLinks);
        }

        public event Action<IEnumerable<PodcastLinkInformation>> Result;
        public event Action<int> SignalTotalCount;
    }

    public class PodcastLinkInformation
    {
        public PodcastLinkInformation(string fileAddress, DateTimeOffset publishDate, string title)
        {
            FileAddress = fileAddress;
            PublishDate = publishDate;
            Title = title;
        }

        public string FileAddress { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public string Title { get; set; }
    }
}

