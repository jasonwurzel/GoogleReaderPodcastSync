namespace GoogleReaderAPI.Http
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;

    internal class HttpSession
    {
        private Dictionary<string, Cookie> cookies = new Dictionary<string, Cookie>();
        private ReaderWriterLock cookiesLock = new ReaderWriterLock();
        private int lockTimeOut = 2000;

        public void ProcessResponse(HttpWebResponse response)
        {
            foreach (var c in response.Cookies) 
            {
                var cookie = c as Cookie;
                if (cookie != null) 
                {
                    if(cookies.ContainsKey(cookie.Name))
                    {
                        cookies[cookie.Name] = cookie;
                    }
                    else
                    {
                        cookiesLock.AcquireWriterLock(lockTimeOut);
                        try
                        {
                            cookies.Add(cookie.Name, cookie);
                        }
                        finally 
                        {
                            cookiesLock.ReleaseWriterLock();    
                        }
                    }
                }
            }
        }

        public void PrepareRequest(HttpWebRequest request) 
        {
            request.CookieContainer = new CookieContainer();
            cookiesLock.AcquireReaderLock(lockTimeOut);

            try
            {
                foreach (var cookie in cookies)
                {
                    request.CookieContainer.Add(cookie.Value);
                }
            }
            finally
            {
                cookiesLock.ReleaseReaderLock();
            }
        }

        public void AttachSIDCookie(string sid)
        {
         //   Cookie cookie = new Cookie("SID", sid);
         //   cookie.Domain = ".google.com";
         //   cookies[cookie.Name] = cookie;
        }
    }
}
