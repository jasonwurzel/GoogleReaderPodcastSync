using System;
using System.Collections.Generic;
using System.Linq;
using Repository;

namespace Flows.DownloadPodcastsFromReaderFlows
{
    public class GetPodcastLinksFromFeed
    {
        private IFeedRepository _repository;
        private int _getFilesFromTheLastXDays;

        public GetPodcastLinksFromFeed(IFeedRepository repository, int getFilesFromTheLastXDays)
        {
            _repository = repository;
            _getFilesFromTheLastXDays = getFilesFromTheLastXDays;
        }

        public void Process(string feedUrl)
        {
            // Den ganzen Tag betrachten!
            var now = DateTime.Now;
            var dateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime minimumPublishDate = dateTime.AddDays(-_getFilesFromTheLastXDays);

            var items = _repository.GetItemsForFeed(feedUrl, minimumPublishDate);

            if (SignalTotalCount != null)
                SignalTotalCount(items.Count());

            Result(items);
        }

        public event Action<IEnumerable<PodcastLinkInformation>> Result;
        public event Action<int> SignalTotalCount;
    }

}

