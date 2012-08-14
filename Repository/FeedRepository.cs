using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using GoogleReaderAPI2;

namespace Repository
{
    public class FeedRepository : IFeedRepository
    {
        private Reader _reader;

        public FeedRepository(Reader reader)
        {
            _reader = reader;
        }
        public IEnumerable<UrlAndFeed> GetFeeds(string label)
        {
            var unreadFeeds = _reader.GetUnreadFeeds();

            List<SyndicationFeed> feeds =
                unreadFeeds.
                    Select(unreadFeed => _reader.GetFeed(unreadFeed.Url, 1)).
                    Where(syndicationFeed => syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == label))).
                    ToList();

            foreach (var syndicationFeed in feeds)
            {
                string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();

                yield return (new UrlAndFeed(url, syndicationFeed.Title.Text));
            }
        }

        public IEnumerable<PodcastLinkInformation> GetItemsForFeed(string feedUrl, DateTime earliestPublishDate)
        {
            var listOfLinks = new List<PodcastLinkInformation>();
            int maxItems = 30;
            var syndicationItems = _reader.GetFeed(feedUrl, maxItems).Items.OrderBy(item => item.PublishDate);
            var actualEarliestPublishDate = syndicationItems.First().PublishDate;
            while (actualEarliestPublishDate > earliestPublishDate)
            {
                maxItems += 10;
                syndicationItems = _reader.GetFeed(feedUrl, maxItems).Items.OrderBy(item => item.PublishDate);
                actualEarliestPublishDate = syndicationItems.First().PublishDate;
            }

            foreach (var item in syndicationItems.Where(item => item.PublishDate > earliestPublishDate))
            {
                //Console.WriteLine(item.PublishDate);
                //Console.WriteLine(item.Title.Text);
                var links = item.Links.Where(l => l.RelationshipType.ToLower() == "enclosure");
                SyndicationItem item1 = item;
                listOfLinks.AddRange(links.Select(l => new PodcastLinkInformation(l.Uri.OriginalString, item1.PublishDate, item1.Title.Text)));
            }

            return listOfLinks;
        }
    }
}