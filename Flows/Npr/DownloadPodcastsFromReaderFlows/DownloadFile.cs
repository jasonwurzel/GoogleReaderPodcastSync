using System;
using System.Net;

namespace Flows.Npr.DownloadPodcastsFromReaderFlows
{
    public class DownloadFile
    {
        private bool _downloadrunning;

        public void Process(RemoteAndLocalAddress remoteAndLocalAddress)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    OnDownloadStarting(remoteAndLocalAddress);
                    _downloadrunning = true;
                    webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
                    webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;
                    //webClient.DownloadFileAsync(new Uri(remoteAndLocalAddress.RemoteAddress), remoteAndLocalAddress.LocalAddress);
                    webClient.DownloadFile(remoteAndLocalAddress.RemoteAddress, remoteAndLocalAddress.LocalAddress);
                    //while (_downloadrunning)
                    //    Thread.Sleep(500);
                    OnDownloadFinished(remoteAndLocalAddress);
                    if (Result != null)
                        Result(remoteAndLocalAddress);
                }
            }
            finally
            {
                _downloadrunning = false;
            }
        }

        public event Action<RemoteAndLocalAddress> Result = address => { };
        public event Action<RemoteAndLocalAddress> OnDownloadStarting = address => { };
        public event Action<RemoteAndLocalAddress> OnDownloadFinished = address => { };

        private void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _downloadrunning = false;
        }

        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Write("\x000D                                  ");
            Console.Write("\x000DReceived {0} percent", (int)(100.0 * e.BytesReceived / e.TotalBytesToReceive));
            //Console.Write(".");
        }
    }
}