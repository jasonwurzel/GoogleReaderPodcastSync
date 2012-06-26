namespace GoogleReaderAPI
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using GoogleReaderAPI.DataContracts;

    public class TestReader : IReader
    {
        #region IReader Members

        public void setProxy(bool isEnabled, string host, int port, string user, string password)
        {
            return;
        }

        public bool enableLogOutput(bool isEnabled)
        {
            return true;
        }

        public IEnumerable<UnreadItem> GetAlreadySeenItems()
        {
            return null;
        }

        public IEnumerable<UnreadItem> GetUnreadItems()
        {
            return null;
        }

        

        public bool SubscribeToFeedUrl(string feedUrl)
        {
            return true;
        }

        public bool UnsubscribeFromFeedUrl(string feedUrl)
        {
            return true;
        }

        public bool SetFeedAsRead(string feedUrl)
        {
            return true;
        }

        public bool SetAsShared(bool currentState, string itemID, string feedUrl, string note)
        {
            return true;
        }

        public bool SendAsEMail(string itemId, string receiver, string body, string subject, bool ccMyself)
        {
            return true;
        }

        public bool SetTags(string itemID, string feedUrl, string note)
        {
            return true;
        }

        public bool SetAsLiked(string itemID, string feedUrl, bool liked)
        {
            return true;
        }

        public IEnumerable<UnreadFeed> GetUnreadFeedsWithItems()
        {
            for (int i = 1; i <= 5; i++) {

                string feedTitle = "Feed" + i;
                string feedUrl = string.Format("http://test-{0}.com", i);

                UnreadFeed result = new UnreadFeed()
                {
                    Title = feedTitle,
                    UnreadCount = i,
                    Url = feedUrl
                };

                for (int c = 1; c <= 5; c++) {
                    result.Items.Add(new UnreadItem() { 
                        Id = Guid.NewGuid().ToString(),
                        ParentFeedTitle = feedTitle,
                        ParentFeedUrl = feedUrl,
                        Title = string.Format("Title {0} {1}", i, c),
                        Summary = string.Format("<body scroll=\"no\">Summary <b>{0} {1}</b>!</body>", i, c)
                    });
                }

                yield return result;
            }
        }

        public bool SetAsRead(string itemId, string feedURL)
        {
            Thread.Sleep(2000);
            return true;
        }

        public bool SetAsStarred(string itemId, string feedURL, bool starred)
        {
            Thread.Sleep(2000);
            return true;
        }

        public IEnumerable<UnreadItem> GetNewUnreadItems( Dictionary<string, string> oldUnreadItemIds)
        {
            oldUnreadItemIds.Clear();
            string url = "http://google.com" + Guid.NewGuid().ToString();
            for (int c = 1; c <= 2; c++)
            {
                yield return new UnreadItem()
                {
                    Id = Guid.NewGuid().ToString(),
                    ParentFeedTitle = url,
                    ParentFeedUrl = url,
                    Title = string.Format("Title {0}", c),
                    Summary = string.Format("<body scroll=\"no\">Summary <b>{0}</b>!</body>", c)
                };
            }
        }


        public int GetUnreadCount() 
        {
            return 200;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
           
        }

        #endregion


        public IEnumerable<UnreadItem> GetUnreadItems(int maxNumberOfItems, bool startWithOldestItem)
        {
            throw new NotImplementedException();
        }
    }
}
