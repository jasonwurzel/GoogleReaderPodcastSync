using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace ConsoleApplication
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
                    Console.WriteLine("Starting Downloading File {0}", remoteAndLocalAddress.LocalAddress);
                    _downloadrunning = true;
                    webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
                    webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;
                    webClient.DownloadFileAsync(new Uri(remoteAndLocalAddress.RemoteAddress), remoteAndLocalAddress.LocalAddress);
                    while (_downloadrunning)
                        Thread.Sleep(500);
                    Console.WriteLine("Finished Downloading File {0}", remoteAndLocalAddress.LocalAddress);
                    Console.WriteLine();
                    if (Result != null)
                        Result(remoteAndLocalAddress);
                }
            }
            finally
            {
                _downloadrunning = false;
            }
        }

        public event Action<RemoteAndLocalAddress> Result;

        private void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine();
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