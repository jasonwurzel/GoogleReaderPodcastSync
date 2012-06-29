using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using GoogleReaderAPI2;

namespace ConsoleApplication
{
    static internal class GetFeedsWithGivenLabel
    {
        public static IEnumerable<UrlAndFeed> Process(Reader reader, string labelName)
        {
            foreach (var unreadFeed in reader.GetUnreadFeeds())
            {
                // Leider Umweg nötig
                var syndicationFeed = reader.GetFeed(unreadFeed.Url, 1);

                if (syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == labelName)))
                {
                    string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();
                    Console.WriteLine("Checking Feed {0} for Items.", url);
                    yield return new UrlAndFeed(url, syndicationFeed);
                }
            }
        }

        internal class UrlAndFeed
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
}