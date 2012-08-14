using System;
using System.Collections.Generic;
using System.IO;
using Repository;

namespace Flows.DownloadPodcastsFromReaderFlows
{
    public class GetRemoteAndLocalAddress
    {
        private IEnumerable<PodcastLinkInformation> _podcastLinks;
        private string _dirPath;
        private Func<PodcastLinkInformation, string> _getLocalFileName;

        public GetRemoteAndLocalAddress(Func<PodcastLinkInformation, string> getLocalFileName)
        {
            _getLocalFileName = getLocalFileName;
        }

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
                List<RemoteAndLocalAddress> list = new List<RemoteAndLocalAddress>();

                foreach (var link in _podcastLinks)
                {
                    string remoteAdress = link.FileAddress;
                    //Console.WriteLine(remoteAdress);
                    string localFileName = _getLocalFileName(link);
                    //string localFileName = link.PublishDate.ToString("yyyyMMddTHHmmss") + "_" + link.Title.ToValidFileName() + ".mp3";
                    var localFilePath = Path.Combine(_dirPath, localFileName);
                    //Console.WriteLine(localFileName);
                    list.Add(new RemoteAndLocalAddress(remoteAdress, localFilePath));
                }

                Result(list);

                _podcastLinks = null;
                _dirPath = null;
            }
        }


        public event Action<IEnumerable<RemoteAndLocalAddress>> Result;
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