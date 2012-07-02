using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using ConsoleApplication;
using GoogleReaderAPI2;
using Tools.DnpExtensions;

namespace ConsoleApplication
{
    public class GetPodcastLinksFromFeed
    {
        private Reader _reader;
        private int _itemsToGetPerFeed;

        public GetPodcastLinksFromFeed(Reader reader, int itemsToGetPerFeed)
        {
            _reader = reader;
            _itemsToGetPerFeed = itemsToGetPerFeed;
        }

        // Better it would be to process only a string (url), but one step at a time!
        public void Process(string feedUrl)
        {
            var listOfLinks = new List<PodcastLinkInformation>();

            foreach (var item in _reader.GetFeed(feedUrl, _itemsToGetPerFeed).Items)
            {
                Console.WriteLine(item.PublishDate);
                Console.WriteLine(item.Title.Text);
                var links = item.Links.Where(l => l.RelationshipType.ToLower() == "enclosure");
                SyndicationItem item1 = item;
                listOfLinks.AddRange(EnumerableExtensions.Select(links, l => new PodcastLinkInformation(l.Uri.OriginalString, item1.PublishDate, item1.Title.Text)));
            }

            Result(listOfLinks);
        }

        public event Action<IEnumerable<PodcastLinkInformation>> Result;
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

