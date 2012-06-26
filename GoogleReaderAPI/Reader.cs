namespace GoogleReaderAPI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ServiceModel.Syndication;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using GoogleReaderAPI.DataContracts;
    using GoogleReaderAPI.Http;

    public class Reader : IReader
    {
        private const int MAX_ITEMS_TO_FETCH = 20;

        private const string GOOGLE_ADDRESS = "https://www.google.com/";
        private const string GOOGLE_API_ADDRESS = GOOGLE_ADDRESS + "reader/api/0/";
        private const string GOOGLE_LOGIN_ADDRESS = GOOGLE_ADDRESS + "accounts/ClientLogin";
        private const string GOOGLE_CMD_TOKEN = "token";
        private const string GOOGLE_CMD_EDIT_TAGS = "edit-tag";
        private const string GOOGLE_CMD_LIST_SUBSCRIPTIONS = "subscription/list";

        #region Factory

        public static IReader CreateTestReader()
        {
            return new TestReader();
        }



        public static IReader CreateReader(string email, string password, string source)
        {
            
            LogWriter.CreateLogfile();
            Properties.Settings.Default.authToken = "";
            const string authUrl = GOOGLE_LOGIN_ADDRESS;

            var reader = new Reader(source);

            string response = HttpClient.SendPostRequest(authUrl, new
            {
                
                service = "reader",
                Email = email,
                Passwd = password,
                source="DesktopGoogleReader",
                accountType = "GOOGLE"
            }, false);

            LogWriter.WriteTextToLogFile("Response from Google");
            LogWriter.WriteTextToLogFile(response);

            string authToken = "";

            try
            {
                LogWriter.WriteTextToLogFile("Trying to get Auth Token");
                authToken = new Regex(@"Auth=(?<authToken>\S+)").Match(response).Result("${authToken}");
                LogWriter.WriteTextToLogFile("AuthToken is " + authToken);
                Properties.Settings.Default.authToken = authToken.Trim();
            }
            catch (Exception e)
            {
                LogWriter.WriteTextToLogFile(e);
                throw new ArgumentException("AuthToken parsing error: " + e.Message);
            }

            return reader;
        }

        #endregion

        #region Infrastructure

        private HttpSession Session { get; set; }
        private string Source { get; set; }

        private Reader(string source)
        {
            this.Source = source;
        }


        private string GetToken() 
        {
            return HttpClient.SendGetRequest(GOOGLE_API_ADDRESS + GOOGLE_CMD_TOKEN, new { }, false).Trim();
        }

        private IEnumerable<UnreadItem> DeserializeItemsJson(string data, Func<string, bool> checkAction)
        {
            LogWriter.WriteTextToLogFile("Starting to deserialize items JSON from Google");
            List<UnreadItem> returnValues = new List<UnreadItem>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = data.Length;
            var obj = serializer.DeserializeObject(data) as Dictionary<string,object>;
            if (obj != null) 
            {
                string ownUserId = obj["id"].ToString();
                
                char[] separators = {'/'};
                string[] temp = ownUserId.Split(separators);
                if(temp.Length > 2) {
                    ownUserId = temp[1];
                }

                var itemsCollection = obj["items"] as object[];
                if (itemsCollection != null) 
                {
                    foreach (var i in itemsCollection) 
                    {
                        var item = i as Dictionary<string, object>;
                        if (item != null)
                        {
                            LogWriter.WriteTextToLogFile("Start parsing next entry");
                            string itemId = item["id"].ToString();
                            LogWriter.WriteTextToLogFile("Item ID is " + itemId);

                            if (!checkAction(itemId))
                            {
                                LogWriter.WriteTextToLogFile("This item is already in the list of known items");
                                continue;
                            }

                            string summaryContent = string.Empty;
                            string enclosureContent = string.Empty;

                            LogWriter.WriteTextToLogFile("Parsing summary");
                            if (item.ContainsKey("summary"))
                            {
                                LogWriter.WriteTextToLogFile("Item contains key summary");
                                var summary = item["summary"] as Dictionary<string, object>;
                                summaryContent = summary["content"].ToString();
                            }
                            else if(item.ContainsKey("content")) 
                            {
                                LogWriter.WriteTextToLogFile("Item contains key content");
                                var summary = item["content"] as Dictionary<string, object>;
                                summaryContent = summary["content"].ToString();
                            }

                            bool liked = false;
                            int likingUsersNumber = 0;
                            LogWriter.WriteTextToLogFile("Parsing likingUsers");
                            if (item.ContainsKey("likingUsers"))
                            {
                                List<string> listOfLikingUser = new List<string>();
                                var lObjList = item["likingUsers"] as object[];
                                foreach (var l in lObjList) 
                                {
                                    LogWriter.WriteTextToLogFile("Parsing single likingUsers entry");
                                    var likingUser = l as Dictionary<string, object>;
                                    listOfLikingUser.Add(likingUser["userId"].ToString());
                                    
                                }
                                likingUsersNumber = listOfLikingUser.Count();

                            }

                            List<string> userLabels = new List<string>();
                            List<string> readerApiTages = new List<string>();
                            List<string> categories = new List<string>();
                            bool starred = false;
                            bool shared = false;
                            bool keptUnread = false;

                            LogWriter.WriteTextToLogFile("Starting parsing categories");
                            if (item.ContainsKey("categories"))
                            {
                                LogWriter.WriteTextToLogFile("Item categories available");
                                var lObjList = item["categories"] as object[];
                                foreach (string category in lObjList)
                                {
                                    LogWriter.WriteTextToLogFile("Category is " + category);
                                    if(category.StartsWith("user")) {
                                        string[] tempArray = category.Split('/');
                                        if (tempArray.Length > 2)
                                        {
                                            LogWriter.WriteTextToLogFile("It is category user - array has a length of " + tempArray.Length);
                                            
                                            if (tempArray[tempArray.Length - 2] == "label")
                                            {
                                                LogWriter.WriteTextToLogFile("It is a label");
                                                userLabels.Add(tempArray[tempArray.Length - 1]);
                                            }
                                            else
                                            {
                                                LogWriter.WriteTextToLogFile("It is a tag");
                                                readerApiTages.Add(tempArray[tempArray.Length - 1]);
                                            }

                                        }
                                        else
                                        {
                                            LogWriter.WriteTextToLogFile("It is a category");
                                            categories.Add(category);
                                        }
                                    }
                                    
                                }

                                LogWriter.WriteTextToLogFile("Parsing categories");
                                var categoriesList = item["categories"] as object[];
                                if (categoriesList != null)
                                {
                                    LogWriter.WriteTextToLogFile("Categories not empty: " + categoriesList.ToString());
                                    LogWriter.WriteTextToLogFile("Start parsing starred");
                                    starred = (categoriesList.FirstOrDefault(o => (o != null && o.ToString().EndsWith("/state/com.google/starred"))) != null);
                                    LogWriter.WriteTextToLogFile("Start parsing shared");
                                    shared = (categoriesList.FirstOrDefault(o => (o != null && o.ToString().EndsWith("/state/com.google/broadcast"))) != null);
                                    LogWriter.WriteTextToLogFile("Start parsing liked");
                                    liked = (categoriesList.FirstOrDefault(o => (o != null && o.ToString().EndsWith("/state/com.google/like"))) != null);
                                    LogWriter.WriteTextToLogFile("Start parsing kept unread");
                                    keptUnread = (categoriesList.FirstOrDefault(o => (o != null && o.ToString().EndsWith("/state/com.google/tracking-kept-unread"))) != null);
                                }


                            }

                            string enclosureType = string.Empty;
                            LogWriter.WriteTextToLogFile("Start parsing enclosure");
                            if (item.ContainsKey("enclosure"))
                            {
                                LogWriter.WriteTextToLogFile("Item has enclosure");
                                var enclosure = item["enclosure"] as object[];
                                LogWriter.WriteTextToLogFile("Enclosure is " + enclosure.ToString());
                                if (enclosure != null && enclosure.Length > 0)
                                {
                                    LogWriter.WriteTextToLogFile("Number of items in enclosure: " + enclosure.Length.ToString());
                                    var enclosureItem = enclosure[0] as Dictionary<string, object>;
                                    if (enclosureItem != null && enclosureItem.ContainsKey("href"))
                                    {
                                        LogWriter.WriteTextToLogFile("It has a link (href)");
                                        enclosureContent = enclosureItem["href"].ToString();
                                        LogWriter.WriteTextToLogFile(enclosureContent);
                                    }
                                    if (enclosureItem != null && enclosureItem.ContainsKey("type"))
                                    {
                                        LogWriter.WriteTextToLogFile("It has a type");
                                        enclosureType = enclosureItem["type"].ToString();
                                        LogWriter.WriteTextToLogFile("Type is " + enclosureType);
                                    }
                                }
                            }

                            string ParentFeedTitle = "";
                            string feedUrl = "";

                            LogWriter.WriteTextToLogFile("Start parsing origin");
                            if (item.ContainsKey("origin"))
                            {
                                LogWriter.WriteTextToLogFile("Item has an origin");
                                var origin = item["origin"] as Dictionary<string, object>;
                                LogWriter.WriteTextToLogFile("Origin is " + origin.ToString());
                                LogWriter.WriteTextToLogFile("Start trying to parse streamId (feedUrl)");
                                feedUrl = origin["streamId"].ToString();
                                LogWriter.WriteTextToLogFile("Stream ID is " + feedUrl);
                                if (feedUrl.StartsWith("feed/"))
                                {
                                    LogWriter.WriteTextToLogFile("Feed url starts with feed/");
                                    feedUrl = feedUrl.Substring(5);
                                }
                                LogWriter.WriteTextToLogFile("Parsing parent title");
                                try
                                {
                                    ParentFeedTitle = decodeHtmlFromJsonObject(origin["title"]);
                                }
                                catch (Exception exp)
                                {
                                    LogWriter.WriteTextToLogFile(exp);
                                    ParentFeedTitle = "";
                                }
                                LogWriter.WriteTextToLogFile("Parent feed title is " + ParentFeedTitle);
                            }
                            else
                            {
                                LogWriter.WriteTextToLogFile("WARN : Feed without origin");
                                throw new Exception("Feed without origin");
                            }
                            

                            string itemUrl = string.Empty;

                            LogWriter.WriteTextToLogFile("Start parsing alternate");
                            if (item.ContainsKey("alternate")) 
                            {
                                LogWriter.WriteTextToLogFile("Alternate is available");
                                var alternate = item["alternate"] as object[];
                                if (alternate != null && alternate.Length > 0) {
                                    var alternateItem = alternate[0] as Dictionary<string, object>;
                                    if(alternateItem!=null && alternateItem.ContainsKey("href"))
                                    {
                                        LogWriter.WriteTextToLogFile("Alternate has a href");
                                        itemUrl = alternateItem["href"].ToString();  
                                        LogWriter.WriteTextToLogFile("itemUrl is " + itemUrl);
                                    }
                                }
                            }

                            long timestamp = 0;
                            string humanReadablePublished = "";

                            LogWriter.WriteTextToLogFile("Start parsing published");
                            if (item.ContainsKey("published"))
                            {
                                LogWriter.WriteTextToLogFile("It has a published");
                                try
                                {
                                    timestamp = Convert.ToInt32(item["published"].ToString());
                                    DateTime myDateObject = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                                    myDateObject = myDateObject.AddSeconds(timestamp);
                                    humanReadablePublished = string.Format("{0} {1}", myDateObject.ToShortDateString(), myDateObject.ToShortTimeString());
                                    LogWriter.WriteTextToLogFile("Was able to convert: " + humanReadablePublished);
                                }
                                catch (Exception e)
                                {
                                    LogWriter.WriteTextToLogFile(e);
                                    timestamp = 0;
                                }


                            }

                            string enhancedSummary = "";
                            

                            enhancedSummary = summaryContent;

                            if (itemUrl != string.Empty)
                            {
                                enhancedSummary += "<p /><p><a style=\"border:1px solid black;padding:0.5em;background-color:#eee;float:right\" href=\"" + itemUrl + "\">Read full article</a></p>";
                            }

                            LogWriter.WriteTextToLogFile("start parsing crawltime");
                            long crawlTime = 0;
                            if (item.ContainsKey("crawlTimeMsec"))
                            {
                                LogWriter.WriteTextToLogFile("It has a crawl time");
                                try
                                {
                                    crawlTime = Convert.ToInt64(item["crawlTimeMsec"].ToString());
                                    LogWriter.WriteTextToLogFile("Successfully cinverted crawltime " + crawlTime);
                                }
                                catch (Exception e)
                                {
                                    LogWriter.WriteTextToLogFile("Convertion of last time crawled failed");
                                    LogWriter.WriteTextToLogFile(e);
                                }
                            }

                            LogWriter.WriteTextToLogFile("Adding item to list:");
                            LogWriter.WriteTextToLogFile(" ID: " + itemId + ", Feed-Url: " + feedUrl);

                            string title = "Unknown";
                            LogWriter.WriteTextToLogFile("Start parsing title");
                            if (item.ContainsKey("title")) {
                                LogWriter.WriteTextToLogFile("Item has a title");
                                title = decodeHtmlFromJsonObject(item["title"]);
                            }

                            yield return new UnreadItem()
                            {
                                Id = itemId,
                                Title = title,
                                Summary = summaryContent,
                                ParentFeedUrl = feedUrl,
                                ParentFeedTitle = ParentFeedTitle,
                                ItemUrl = itemUrl,
                                Enclosure = enclosureContent,
                                EnclosureType = enclosureType,
                                Starred = starred,
                                Published = timestamp,
                                HumanReadablePublished = humanReadablePublished,
                                EnhancedSummary = enhancedSummary,
                                CrawlTime = crawlTime,
                                Liked = liked,
                                Shared = shared,
                                NumberOfLikingUsers = likingUsersNumber,
                                UserLabels = userLabels,
                                ReaderApiTags = readerApiTages,
                                Categories = categories,
                                alreadySeenByUser = false
                            };
                        }
                    }
                }
            }
        }

        private string decodeHtmlFromJsonObject(object o) 
        {
            if (o != null) 
            {
                string cleanedString =  HttpUtility.HtmlDecode(o.ToString());
                cleanedString = Regex.Replace(cleanedString, "<.*?>", string.Empty);
                return cleanedString;
            }

            return string.Empty;
        }

        #endregion

        #region API calling methods

        private string GetSubscriptions()
        {
            return HttpClient.SendGetRequest(GOOGLE_API_ADDRESS + GOOGLE_CMD_LIST_SUBSCRIPTIONS, new
            {
                output = "xml",
                ck = DateTime.Now.ToString(),
                client = this.Source
            }, false);
        }

        private string GetUnread()
        {
            return HttpClient.SendGetRequest(@"https://www.google.com/reader/api/0/unread-count", new
            {
                all = "true",
                output = "xml",
                ck = DateTime.Now.ToString(),
                client = this.Source
            }, false);
        }


        public int GetUnreadCount()
        {
            LogWriter.WriteTextToLogFile("Starting GetUnreadCount");
            string content = HttpClient.SendGetRequest(@"https://www.google.com/reader/api/0/unread-count", new
            {
                all = "true",
                output = "xml",
                ck = DateTime.Now.ToString(),
                client = this.Source
            }, false);

             var c = from f in XDocument.Parse(content).XPathSelectElements("//object")
                     let name = f.XPathSelectElement("string[@name='id']")
                     where name != null && name.Value.EndsWith("/state/com.google/reading-list")
                     select int.Parse(f.XPathSelectElement("number[@name='count']").Value);


            LogWriter.WriteTextToLogFile(c.Sum().ToString() + " new unread");
             return c.Sum();
        }

        #endregion

        #region Public methods


        public bool SetAsRead(string itemId, string feedURL)
        {
            string result = HttpClient.SendPostRequest(GOOGLE_API_ADDRESS + GOOGLE_CMD_EDIT_TAGS, new
            {
                i = itemId,
                a = "user/-/state/com.google/read",
                r = "user/-/state/com.google/kept-unread",
                ac = "edit",
                T = GetToken()
            }, false);

            return (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "ok");
        }


        public bool SetFeedAsRead(string feedURL)
        {
            var allItems = GetUnreadFeedItems(feedURL);
            foreach (UnreadItem i in allItems)
            {
                SetAsRead(i.Id, feedURL);
                
            }
            return true;
        }

        public bool SetAsShared(bool currentState, string itemId, string feedUrl, string sharedNote)
        {

            object requestBody;
            if (!currentState)
            {
                requestBody = new
                {
                    s = "feed/" + feedUrl,
                    i = itemId,
                    ac = "edit-tags",
                    a = "user/-/state/com.google/broadcast",
                    T = GetToken()
                };
            }
            else
            {
                requestBody = new
                {
                    s = "feed/" + feedUrl,
                    i = itemId,
                    ac = "edit-tags",
                    r = "user/-/state/com.google/broadcast",
                    T = GetToken()
                };
            }


                string result = HttpClient.SendPostRequest(@"https://www.google.com/reader/api/0/edit-tag", requestBody, false);

            return (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "ok");
        }

        public bool SetTags(string itemId, string feedUrl, string newTags)
        {
            char[] separator  = {','};
            string[] tagsArray = newTags.Split(separator,StringSplitOptions.RemoveEmptyEntries);
            object requestBody;

            bool success = true;

            foreach(string tag in tagsArray) {
                requestBody = new
                {
                    s = "feed/" + feedUrl,
                    i = itemId,                    
                    a = "user/-/label/" + tag,
                    T = GetToken()
                };


                string result = HttpClient.SendPostRequest(@"https://www.google.com/reader/api/0/edit-tag", requestBody, false);

                if (!(!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "ok")) 
                {
                    success = false;
                }
            }

            return success;
        }

        public bool SetAsLiked(string itemId, string feedURL, bool currentLikeState)
        {
            string result = "";
            if (!currentLikeState)
            {

                result = HttpClient.SendPostRequest(@"https://www.google.com/reader/api/0/edit-tag", new
                {
                    s = "feed/" + feedURL,
                    i = itemId,
                    a = "user/-/state/com.google/like",
                    T = GetToken()

                }, false);
            }
            else
            {
                // remoce the like tag
                result = HttpClient.SendPostRequest(@"https://www.google.com/reader/api/0/edit-tag", new
                {
                    s = "feed/" + feedURL,
                    i = itemId,
                    r = "user/-/state/com.google/like",
                    T = GetToken()

                }, false);
            }

            return (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "ok");
        }

        public bool SubscribeToFeedUrl(string feedURL)
        {
            string result = "";

            try {
                result = HttpClient.SendPostRequest(@"https://www.google.com/reader/api/0/subscription/edit", new
                {
                    s = "feed/" + feedURL,
                    ac = "subscribe",
                    T = GetToken()

                }, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "ok");
        }

        public bool UnsubscribeFromFeedUrl(string feedURL)
        {
            string result = "";

            try
            {
                result = HttpClient.SendPostRequest(@"https://www.google.com/reader/api/0/subscription/edit", new
                {
                    s = "feed/" + feedURL,
                    ac = "unsubscribe",
                    T = GetToken()

                }, false);
            }
            catch (Exception e)
            {
                LogWriter.WriteTextToLogFile(e);
            }

            return (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "ok");
        }

        public bool SetAsStarred(string itemId, string feedURL, bool starred)
        {
            object requestBody;

            if (starred)
            {
                requestBody = new
                {
                    s = "feed/" + feedURL,
                    i = itemId,
                    ac = "edit-tags",
                    a = "user/-/state/com.google/starred",
                    T = GetToken()
                };
            }
            else
            {
                requestBody = new
                {
                    s = "feed/" + feedURL,
                    i = itemId,
                    ac = "edit-tags",
                    r = "user/-/state/com.google/starred",
                    T = GetToken()
                };
            }

            string result = HttpClient.SendPostRequest(@"https://www.google.com/reader/api/0/edit-tag", requestBody, false);

            return (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "ok");
        }

        public bool SendAsEMail(string itemId, string receiver, string body, string subject, bool ccMyself)
        {
            object requestBody = new
                {
                    i = itemId,
                    emailTo = receiver,
                    comment = body,
                    subject = subject,
                    ccMe = ccMyself.ToString().ToLower(),
                    T = GetToken()
                };

            string result = HttpClient.SendPostRequest(@"https://www.google.com/reader/email-this", requestBody, false);

            return (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "ok");
        }


        public IEnumerable<UnreadFeed> GetUnreadFeeds()
        {
            string unreadContent = GetUnread();
            string subscriptionContent = GetSubscriptions();

            var unreadFeeds =
                    from f in XDocument.Parse(unreadContent).XPathSelectElements("//list[@name='unreadcounts']/object")
                    where f.XPathSelectElement("string[@name='id']").Value.StartsWith("feed/")
                    select f;

            var subscriptionFeeds =
                    from f in XDocument.Parse(subscriptionContent).XPathSelectElements("//list[@name='subscriptions']/object")
                    where f.XPathSelectElement("string[@name='id']").Value.StartsWith("feed/")
                    select f;


            return from u in unreadFeeds
                   join s in subscriptionFeeds on u.XPathSelectElement("string[@name='id']").Value equals
                       s.XPathSelectElement("string[@name='id']").Value
                   select new UnreadFeed()
                              {
                                  Url =
                                      s.XPathSelectElement("string[@name='id']").Value.Contains("feed/")
                                          ? s.XPathSelectElement("string[@name='id']").Value.Remove(0, "feed/".Length)
                                          : s.XPathSelectElement("string[@name='id']").Value,
                                  UnreadCount = int.Parse(u.XPathSelectElement("number[@name='count']").Value),
                                  Title = s.XPathSelectElement("string[@name='title']").Value
                              };
        }

        public SyndicationFeed GetFeed(string url, int topN)
        {
            string feedUrl = string.Format(@"https://www.google.com/reader/atom/feed/{0}", url);
            string content = HttpClient.SendGetRequest(feedUrl, new { n = topN }, false);

            return SyndicationFeed.Load(XmlReader.Create(new StringReader(content)));
        }

        public IEnumerable<UnreadFeed> GetUnreadFeedsWithItems()
        {
            return GetUnreadFeedsWithItems(MAX_ITEMS_TO_FETCH);
        }

        public IEnumerable<UnreadFeed> GetUnreadFeedsWithItems(int maxItems)
        {
            var items = GetUnreadItems(maxItems);
            var itemGrounps = items.GroupBy(e=>e.ParentFeedUrl);
            var result = new List<UnreadFeed>();

            foreach(var group in itemGrounps){
                UnreadFeed feed = new UnreadFeed();
                feed.Url = group.Key;
                feed.UnreadCount = group.Count();
                bool titleAdded = false;
                group.ToList().ForEach(e => {
                    if (!titleAdded) {
                        feed.Title = e.ParentFeedTitle;
                        feed.Items.Add(e);
                    }
                });
                result.Add(feed);
            }

            return result;
        }

        public IEnumerable<UnreadItem> GetUnreadFeedItems(string feedUrl)
        {
            var items = GetUnreadItems();
            var result =
               (from t in items
                where t.ParentFeedUrl == feedUrl
                select t
               ).ToList();

            return result;
        }

        public IEnumerable<UnreadItem> GetAlreadySeenItems()
        {
            var items = GetUnreadItems();
            var result =
               (from t in items
                where t.alreadySeenByUser == true
                select t
               ).ToList();

            return result;
        }

        private string CoreGetUnreadItems(int topN, bool startWithOldestItem)
        {
            const string url = @"https://www.google.com/reader/api/0/stream/contents/user/-/state/com.google/reading-list";
            LogWriter.WriteTextToLogFile("Getting JSON response from Google now in CoreGetUnread");
            if (startWithOldestItem)
            {
                return HttpClient.SendGetRequest(url, new { r = "o", n = topN, xt = "user/-/state/com.google/read", output = "json" }, false);
            }
            else
            {
                return HttpClient.SendGetRequest(url, new { r = "d",  n = topN, xt = "user/-/state/com.google/read", output = "json" }, false);
            }


           /* if (newestKnownCrawlTime == 0)
            {
                return HttpClient.SendGetRequest(url, new {r = "o", n = topN, xt = "user/-/state/com.google/read", output = "json" }, Session, false);
            }
            else
            {
                return HttpClient.SendGetRequest(url, new {r = "d", ot = newestKnownCrawlTime,  n = topN, xt = "user/-/state/com.google/read", output = "json" }, Session, false);
            }
            * */
        }

        public IEnumerable<UnreadItem> GetItemsByLabel(string label)
        {
            string data = "";

            //string url = String.Format(@"http://www.google.com/reader/api/0/stream/contents/user/-/label/{0}?ot=1253066400&r=n&xt=user/-/com.google/read&n=20&ck=<timeStamp>&client=<application Name>", label);
            string url = String.Format(@"http://www.google.com/reader/api/0/stream/contents/user/-/label/{0}?", label);

            data = HttpClient.SendGetRequest(url, new { r = "d", xt = "user/-/state/com.google/read", output = "json", ck = DateTime.Now.Ticks, ot = DateTime.Now.AddMonths(-1).Ticks }, false);

            return DeserializeItemsJson(data, (e => true));
        }

        public IEnumerable<UnreadItem> GetUnreadItems(int maxItems)
        {
            return GetUnreadItems(maxItems, true);
        }

        public IEnumerable<UnreadItem> GetUnreadItems()
        {
            return GetUnreadItems(MAX_ITEMS_TO_FETCH, true);
        }

        public IEnumerable<UnreadItem> GetUnreadItems(int maxNumberOfItems, bool startWithOldestItem) 
        {
           /* List<UnreadItem> NewItems = new List<UnreadItem>();
            IEnumerable<UnreadItem> LastUpdate = new List<UnreadItem>();
            long newestItemCrawlTime = 0;
            */

            //int topN = Math.Min(maxNumberOfItems, GetUnreadCount());
            int topN = maxNumberOfItems;
            LogWriter.WriteTextToLogFile("Fetching now the " + topN.ToString() + " newest items");

           /* do
            {
                LastUpdate = new List<UnreadItem>();
                LastUpdate = DeserializeItemsJson(CoreGetUnreadItems(20, newestItemCrawlTime), (e => true));
                NewItems.AddRange(LastUpdate);
                if(LastUpdate != null) {
                    newestItemCrawlTime = NewItems.Max(item => item.CrawlTime);
                }
                else
                {
                    newestItemCrawlTime = 0;
                }
                
            } while (LastUpdate.Count() >= 20);
            */
            return DeserializeItemsJson(CoreGetUnreadItems(topN, startWithOldestItem), (e => true));
            
        }


        public IEnumerable<UnreadItem> GetNewUnreadItems(Dictionary<string, string> oldUnreadItemIds)
        {

            //int topN = Math.Min(100,GetUnreadCount());
            int topN = GetUnreadCount();

            Func<string, bool> checkAction = ((e) =>
            {
                if (oldUnreadItemIds.ContainsKey(e))
                {
                    oldUnreadItemIds.Remove(e);
                    return false;
                }
                return true;
            });

            return DeserializeItemsJson(CoreGetUnreadItems(topN,true), checkAction);



            /*
            int topN = GetUnreadCount();

            IEnumerable<UnreadItem> NewItems = new List<UnreadItem>();
            IEnumerable<UnreadItem> LastUpdate = new List<UnreadItem>();

            long newestItemCrawlTime = 0;

            Func<string, bool> checkAction = ((e) => {
                if (oldUnreadItemIds.ContainsKey(e)) {
                    oldUnreadItemIds.Remove(e);
                    return false;
                }
                return true;
            });

            do
            {
                LastUpdate = DeserializeItemsJson(CoreGetUnreadItems(20, newestItemCrawlTime), checkAction);
                NewItems.Concat(LastUpdate);
                newestItemCrawlTime = NewItems.Max(item => item.CrawlTime);
            } while (LastUpdate.Count() >= 20);

            return NewItems; */
        }

        #endregion

        public bool enableLogOutput(bool isEnabled)
        {
            Properties.Settings.Default.enableLogging = isEnabled;
            return true;
        }

        public void Dispose()
        {
        }
    }
}