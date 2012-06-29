using System;
using System.IO;
using Tools;

namespace ConsoleApplication
{
    public class GetRemoteAndLocalAddress
    {
        private PodcastLinkInformation _podcastLink;
        private string _dirPath;

        public void ProcessDirPath(string dirPath)
        {
            _dirPath = dirPath;
            ProcessDirPathAndPodcastInformation();
        }

        public void ProcessPodcastLinkInformation(PodcastLinkInformation podcastLink)
        {
            _podcastLink = podcastLink;
            ProcessDirPathAndPodcastInformation();
        }

        private void ProcessDirPathAndPodcastInformation()
        {
            if (_dirPath != null && _podcastLink != null)
            {
                string remoteAdress = _podcastLink.FileAddress;
                Console.WriteLine(remoteAdress);
                string localFileName = _podcastLink.PublishDate.ToString("yyyyMMddTHHmmss") + "_" + _podcastLink.Title.ToValidFileName() + ".mp3";
                var localFilePath = Path.Combine(_dirPath, localFileName);
                Console.WriteLine(localFileName);
                Result(new RemoteAndLocalAddress(remoteAdress, localFilePath));

                _podcastLink = null;
                _dirPath = null;
            }
        }


        public event Action<RemoteAndLocalAddress> Result;
    }

    public class RemoteAndLocalAddress
    {
        public RemoteAndLocalAddress(string remoteAdress, string localFilePath)
        {
            RemoteAddress = remoteAdress;
            LocalAddress = localFilePath;
        }

        public string RemoteAddress { get; set; }
        public string LocalAddress { get; set; }
    }

}