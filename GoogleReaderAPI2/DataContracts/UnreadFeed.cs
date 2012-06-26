using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System;

namespace GoogleReaderAPI2.DataContracts
{
    [DebuggerDisplay("Title={Title}, UnreadCount={UnreadCount}, Url={Url}")]
    public class UnreadFeed : System.ComponentModel.INotifyPropertyChanged
    {
        public UnreadFeed() 
        {
            Items = new ObservableCollection<UnreadItem>();
            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(Items_CollectionChanged);
        }

        void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UnreadCount = Items.Count;
        }

        public string FavIcon
        {
            get
            {
                return GetFavicon(this.Url);
            }
        }

        private string GetFavicon(string Inurl)
        {
            Uri url = new Uri(Inurl);
            string urlHost = url.Host;
            Image BookmarkIcon = null;
            if (url.HostNameType == UriHostNameType.Dns)
            {
                string iconUrl = "http://" + urlHost + "/favicon.ico";
                try
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(iconUrl);
                    System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    System.IO.Stream stream = response.GetResponseStream();
                    BookmarkIcon = Image.FromStream(stream);
                    return iconUrl;
                }
                catch
                {
                    return "pack://application:,,,/Resources/images/feed.png";
                }
               
            }
            else
            {
                return "pack://application:,,,/Resources/images/feed.png";
            }
        }

        public string Url { get; set; }

        public string Title { get; set; }

        public UnreadFeed MySelf 
        {
            get 
            { 
                return this; 
            }
        }

        public int UnreadCount { 
            get 
            {
                return _unreadCount;
            }
            set 
            {
                if (_unreadCount != value) 
                {
                    _unreadCount = value;
                    OnPropertyChanged("MySelf");
                }
            }
        }
        private int _unreadCount;

        public ObservableCollection<UnreadItem> Items { get; private set; }


        protected void OnPropertyChanged(string propertyName) 
        {
            if (PropertyChanged != null) 
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}