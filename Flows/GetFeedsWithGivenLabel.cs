using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using GoogleReaderAPI2;

namespace ConsoleApplication
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
            foreach (var unreadFeed in reader.GetUnreadFeeds())
            {
                // Leider Umweg nötig
                var syndicationFeed = reader.GetFeed(unreadFeed.Url, 1);

                if (syndicationFeed.Items.Any(item => item.Categories.Any(c => c.Label == _label)))
                {
                    if (OnFeedFound != null)
                        OnFeedFound();
                    string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();
                    Console.WriteLine("Checking Feed {0} for Items.", url);

                    Result(new UrlAndFeed(url, syndicationFeed));
                }
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
                    OnFeedFound();
                    string url = syndicationFeed.Links.First(l => l.RelationshipType == "self").Uri.ToString();
                    Console.WriteLine("Checking Feed {0} for Items.", url);

                    yield return new UrlAndFeed(url, syndicationFeed);
                }
            }
        } 

        public event Action<UrlAndFeed> Result;
        public event Action OnFeedFound;


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