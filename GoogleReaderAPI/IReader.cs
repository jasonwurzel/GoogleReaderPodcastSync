using System;
using System.Collections.Generic;

namespace GoogleReaderAPI
{
    public interface IReader : IDisposable
    {
        //IEnumerable<UnreadFeed> GetUnreadFeedsWithItems();
        //bool SetAsRead(string itemId, string feedURL);
        //bool SetFeedAsRead(string feedURL);
        //bool SetAsStarred(string itemId, string feedURL, bool starred);
        //bool SetAsLiked(string itemId, string feedURL, bool currentLikeState);
        //bool SetAsShared(bool currentState, string itemId, string feedUrl, string sharedNote);
        //bool SetTags(string itemId, string feedUrl, string tags);
        //IEnumerable<UnreadItem> GetNewUnreadItems(Dictionary<string, string> oldUnreadItemIds);
        //int GetUnreadCount();
        //IEnumerable<UnreadItem> GetUnreadItems();
        //IEnumerable<UnreadItem> GetUnreadItems(int maxNumberOfItems, bool startWithOldestItem);
        //bool SubscribeToFeedUrl(string feedURL);
        //bool UnsubscribeFromFeedUrl(string feedURL);
        //IEnumerable<UnreadItem> GetAlreadySeenItems();
        //bool enableLogOutput(bool isEnabled);
        //bool SendAsEMail(string itemId, string receiver, string body, string subject, bool ccMyself);
    }
}
