using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace GoogleReaderAPI.Http
{
    internal class HttpClient
    {
        #region Public

        public static string SendPostRequest(string url, object data, bool allowAutoRedirect)
        {
            try
            {
                string formData = string.Empty;
                HttpClient.GetProperties(data).ToList().ForEach(x =>
                {
                    formData += string.Format("{0}={1}&", x.Key, x.Value);
                });
                formData = formData.TrimEnd('&');

                //url = ProcessUrl(url);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "POST";
                request.AllowAutoRedirect = allowAutoRedirect;
                request.Accept = "*/*";
                request.UserAgent = "Desktop Google Reader (http://desktopgooglereader.codeplex.com/)";
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] encodedData = Encoding.ASCII.GetBytes(formData);
                request.ContentLength = encodedData.Length;


                if (Properties.Settings.Default.authToken != "")
                {
                    request.Headers.Add("Authorization:GoogleLogin auth=" + Properties.Settings.Default.authToken);
                }

                using (Stream newStream = request.GetRequestStream())
                {
                    newStream.Write(encodedData, 0, encodedData.Length);
                }
                return GetResponse(request);
            }
            catch (System.Exception e)
            {
                LogWriter.WriteTextToLogFile(e);
                return "";
            }
        }


        public static string SendGetRequest(string url, object data, bool allowAutoRedirect)
        {
            string paramData = string.Empty;
            HttpClient.GetProperties(data).ToList().ForEach(x =>
            {
                paramData += string.Format("{0}={1}&", x.Key, x.Value);
            });
            paramData = paramData.TrimEnd('&');

            url = ProcessUrl(url);

            HttpWebRequest request = paramData.Length > 0 ? (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", url, paramData)) : (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = allowAutoRedirect;
            if (Properties.Settings.Default.authToken != "")
            {
                request.Headers.Add("Authorization:GoogleLogin auth=" + Properties.Settings.Default.authToken);
            }
            return GetResponse(request);
        }

        #endregion

        #region Private

        private static string ProcessUrl(string url) 
        {
            string quesytionMarkSymbol = "?";
            if (url.Contains(quesytionMarkSymbol))
            {
                url = url.Replace(quesytionMarkSymbol, System.Web.HttpUtility.UrlEncode(quesytionMarkSymbol));
            }
            return url;
        }

        private static string GetResponse(HttpWebRequest request)
        {
            
            HttpWebResponse response;
            try
            {
                WebResponse responseTemp = (HttpWebResponse)request.GetResponse();
                response = (HttpWebResponse)responseTemp;
                LogWriter.WriteTextToLogFile("Response code: " + response.StatusCode);
                LogWriter.WriteTextToLogFile("Response desc: " + response.StatusDescription);
            }
            catch (System.Exception e)
            {
                // some proxys have problems with Continue-100 headers
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.Expect100Continue = false; 
                System.Net.ServicePointManager.Expect100Continue = false;
                HttpWebResponse responseTemp = (HttpWebResponse)request.GetResponse();
                response = responseTemp;
                LogWriter.WriteTextToLogFile("2nd Response code: " + response.StatusCode);
                LogWriter.WriteTextToLogFile("2nd Response desc: " + response.StatusDescription);
                LogWriter.WriteTextToLogFile("2nd error message: " + e.Message);
            }

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string result = reader.ReadToEnd();
                LogWriter.WriteTextToLogFile("Result (partly): " + result.Substring(0, System.Math.Min(100,result.Length)));
                //if (result.Contains(Resources.GoogleAuthErrorMessage) || result.Contains(Resources.GoogleRequiredFieldBlankErrorMessage))
                //    throw new GoogleAuthenticationException();
                response.Close();
                
                return result;
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetProperties(object o)
        {
            foreach (var p in o.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) 
            {
                yield return new KeyValuePair<string, string>(p.Name.TrimStart('_'), System.Web.HttpUtility.UrlEncode(p.GetValue(o, null).ToString()));    
            }
        }

        #endregion
    }
}
