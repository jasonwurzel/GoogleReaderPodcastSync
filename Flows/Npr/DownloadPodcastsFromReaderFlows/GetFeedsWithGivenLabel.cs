using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using GoogleReaderAPI2;

namespace Flows.Npr.DownloadPodcastsFromReaderFlows
{
    public class GetFeedsWithGivenLabel
    {
        private Reader _reader;

        public GetFeedsWithGivenLabel(Reader reader)
        {
            _reader = reader;
        }

        public void Process(string label, Action<UrlAndFeed> continueWith)
        {
            var unreadFeeds = _reader.GetUnreadFeeds();

            List<SyndicationFeed> feeds =
                unreadFeeds.
                Select(unreadFeed => _reader.GetFeed(unreadFeed.Url, 1)).
                Where(syndicationFeed => syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == label))).
                ToList();

            //if (SignalTotalCount != null)
            //    SignalTotalCount(feeds.Count());

            foreach (var syndicationFeed in feeds)
            {
                string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();

                //if (OnFeedFound != null)
                //    OnFeedFound(url);

                continueWith(new UrlAndFeed(url, syndicationFeed));
            }
        }

        //public event Action<int> SignalTotalCount;
        //public event Action<string> OnFeedFound;
    }

    public class UrlAndFeed
    {
        public UrlAndFeed(string url, SyndicationFeed syndicationFeed)
        {
            Url = url;
            Feed = syndicationFeed;
        }

        public SyndicationFeed Feed { get; set; }

        public string Url { get; set; }
    }

}