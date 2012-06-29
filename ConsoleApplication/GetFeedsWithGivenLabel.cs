using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using GoogleReaderAPI2;

namespace ConsoleApplication
{
    public class GetFeedsWithGivenLabel
    {
        private Reader _reader;

        public GetFeedsWithGivenLabel(Reader reader)
        {
            _reader = reader;
        }

        public void Process(string labelName)
        {
            foreach (var unreadFeed in _reader.GetUnreadFeeds())
            {
                // Leider Umweg nötig
                var syndicationFeed = _reader.GetFeed(unreadFeed.Url, 1);

                if (syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == labelName)))
                {
                    string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();
                    Console.WriteLine("Checking Feed {0} for Items.", url);

                    Result(new UrlAndFeed(url, syndicationFeed));
                }
            }
        }

        public event Action<UrlAndFeed> Result;


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