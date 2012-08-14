using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using Repository;

namespace Flows.DownloadPodcastsFromReaderFlows
{
    public class GetFeedsWithGivenLabel
    {
        private string _label;

        public GetFeedsWithGivenLabel(string label)
        {
            _label = label;
        }

        public void Process(IFeedRepository reader)
        {
            var feeds = reader.GetFeeds(_label);

            if (SignalTotalCount != null)
                SignalTotalCount(feeds.Count());

            foreach (var urlAndFeed in feeds)
            {
                if (OnFeedFound != null)
                    OnFeedFound(urlAndFeed.Url);

                Result(urlAndFeed);
            }
        }

        public event Action<UrlAndFeed> Result;
        public event Action<int> SignalTotalCount;
        public event Action<string> OnFeedFound;


    }


}