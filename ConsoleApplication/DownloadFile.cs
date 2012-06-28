using System;
using System.Net;
using System.Threading;

static internal class DownloadFile
{
    private static bool _downloadrunning;

    public static void Process(string localFilePath, string remoteAdress)
    {
        try
        {
            using (WebClient webClient = new WebClient())
            {
                _downloadrunning = true;
                webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri(remoteAdress), localFilePath);
                while (_downloadrunning)
                    Thread.Sleep(500);
            }
        }
        finally
        {
            _downloadrunning = false;
        }
    }

    private static void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        Console.WriteLine();
        _downloadrunning = false;
    }

    private static void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        Console.Write("\x000D                                  ");
        Console.Write("\x000DReceived {0} percent", (int)(100.0 * e.BytesReceived / e.TotalBytesToReceive));
    }
}