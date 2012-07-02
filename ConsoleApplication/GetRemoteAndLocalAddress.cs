using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Tools;

namespace ConsoleApplication
{
    public class GetRemoteAndLocalAddress
    {
        private IEnumerable<PodcastLinkInformation> _podcastLinks;
        private string _dirPath;

        public void ProcessDirPath(string dirPath)
        {
            _dirPath = dirPath;
            ProcessDirPathAndPodcastInformation();
        }

        public void ProcessPodcastLinkInformation(IEnumerable<PodcastLinkInformation> podcastLinks)
        {
            _podcastLinks = podcastLinks;
            ProcessDirPathAndPodcastInformation();
        }

        private void ProcessDirPathAndPodcastInformation()
        {
            if (_dirPath != null && _podcastLinks != null)
            {
                foreach (var link in _podcastLinks)
                {
                    string remoteAdress = link.FileAddress;
                    Console.WriteLine(remoteAdress);
                    string localFileName = link.PublishDate.ToString("yyyyMMddTHHmmss") + "_" + link.Title.ToValidFileName() + ".mp3";
                    var localFilePath = Path.Combine(_dirPath, localFileName);
                    Console.WriteLine(localFileName);
                    Result(new RemoteAndLocalAddress(remoteAdress, localFilePath));
                }
                _podcastLinks = null;
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