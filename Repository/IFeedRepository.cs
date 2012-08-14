using System;
using System.Collections.Generic;

namespace Repository
{
    public interface IFeedRepository
    {
        IEnumerable<UrlAndFeed> GetFeeds(string label);
        IEnumerable<PodcastLinkInformation> GetItemsForFeed(string feedUrl, DateTime earliestPublishDate);
    }
}