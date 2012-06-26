
using System.Collections.Generic;

namespace GoogleReaderAPI2.DataContracts
{
    public class UnreadItem
    {
        public string Id { get; set; }
        public string Title {
            get
            {
                return _title.Replace("\r", "").Replace("\n", "");
            }
            set
            {
                _title = value;
            }
        }
        public string Summary { get; set; }
        public string EnhancedSummary { get; set; }
        public string ParentFeedUrl { get; set; }
        public string ParentFeedTitle { get; set; }
        public string ItemUrl { get; set; }
        public string Enclosure { get; set; }
        public string EnclosureType { get; set; }
        public long Published { get; set; }
        public string HumanReadablePublished { get; set; }
        public bool Starred { get; set; }
        public bool Liked { get; set; }
        public bool Shared { get; set; }
        public int NumberOfLikingUsers { get; set; }
        public int SnarlNotificationId;
        public bool alreadySeenByUser  { get; set; }
        public long CrawlTime { get; set; }
        public List<string> UserLabels { get; set; }
        public List<string> ReaderApiTags { get; set; }
        public List<string> Categories { get; set; }

        private string _title = "";

        ~UnreadItem()
        {
            
        }


    }
}
