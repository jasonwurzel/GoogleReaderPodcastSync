using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleApplication;
using GoogleReaderAPI2;

namespace ConsoleApplication
{
    internal class PodcastLinkInformation
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

static internal class GetPodcastLinksFromFeed
{
    public static IEnumerable<PodcastLinkInformation> Process(Reader reader, string feedUrl, int itemsToGetPerFeed)
    {
        foreach (var item in reader.GetFeed(feedUrl, itemsToGetPerFeed).Items)
        {
            Console.WriteLine(item.PublishDate);
            Console.WriteLine(item.Title.Text);
            Console.WriteLine("File:");
            var links = item.Links.Where(l => l.RelationshipType.ToLower() == "enclosure");
            foreach (var syndicationLink in links)
            {
                yield return new PodcastLinkInformation(syndicationLink.Uri.OriginalString, item.PublishDate, item.Title.Text);
            }
        }
    }
}