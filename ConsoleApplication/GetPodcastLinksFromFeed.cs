using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApplication;
using GoogleReaderAPI2;

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
            foreach (var item in _reader.GetFeed(feedUrl, _itemsToGetPerFeed).Items)
            {
                Console.WriteLine(item.PublishDate);
                Console.WriteLine(item.Title.Text);
                Console.WriteLine("File:");
                var links = item.Links.Where(l => l.RelationshipType.ToLower() == "enclosure");
                Result(links.Select(l => new PodcastLinkInformation(l.Uri.OriginalString, item.PublishDate, item.Title.Text)));
            }
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

