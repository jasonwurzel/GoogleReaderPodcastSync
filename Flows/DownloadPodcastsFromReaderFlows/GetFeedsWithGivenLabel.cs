using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using GoogleReaderAPI2;

namespace Flows.DownloadPodcastsFromReaderFlows
{
    public class GetFeedsWithGivenLabel
    {
        private string _label;

        public GetFeedsWithGivenLabel(string label)
        {
            _label = label;
        }

        public void Process(Reader reader)
        {
            var unreadFeeds = reader.GetUnreadFeeds();

            List<SyndicationFeed> feeds =
                unreadFeeds.
                Select(unreadFeed => reader.GetFeed(unreadFeed.Url, 1)).
                Where(syndicationFeed => syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == _label))).
                ToList();

            if (SignalTotalCount != null)
                SignalTotalCount(feeds.Count());

            foreach (var syndicationFeed in feeds)
            {
                string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();

                if (OnFeedFound != null)
                    OnFeedFound(url);

                Result(new UrlAndFeed(url, syndicationFeed));
            }
        }

        /// <summary>
        /// Testweise für NPR. TODO: Möglichkeit, das Process/Result Muster zu verwenden?
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IEnumerable<UrlAndFeed> ProcessNPR(Reader reader)
        {
            foreach (var unreadFeed in reader.GetUnreadFeeds())
            {
                // Leider Umweg nötig
                var syndicationFeed = reader.GetFeed(unreadFeed.Url, 1);

                if (syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == _label)))
                {
                    string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();
                    OnFeedFound(url);

                    yield return new UrlAndFeed(url, syndicationFeed);
                }
            }
        } 

        public event Action<UrlAndFeed> Result;
        public event Action<int> SignalTotalCount;
        public event Action<string> OnFeedFound;


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