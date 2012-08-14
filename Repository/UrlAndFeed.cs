namespace Repository
{
    public class UrlAndFeed
    {
        public UrlAndFeed(string url, string feedTitle)
        {
            Url = url;
            FeedTitle = feedTitle;
        }

        public string FeedTitle { get; private set; }

        public string Url { get; private set; }
    }

}