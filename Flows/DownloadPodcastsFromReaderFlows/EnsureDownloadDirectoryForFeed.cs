using System;
using System.IO;
using System.ServiceModel.Syndication;
using Tools;

namespace Flows.DownloadPodcastsFromReaderFlows
{
    public class EnsureDownloadDirectoryForFeed
    {
        private string _baseDirPath;

        public EnsureDownloadDirectoryForFeed(string baseDirPath)
        {
            _baseDirPath = baseDirPath;
        }

        public void Process(SyndicationFeed syndicationFeed)
        {
            string dirPath = Path.Combine(_baseDirPath, syndicationFeed.Title.Text.ToValidDirName());
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            
            Result(dirPath);
        }

        public event Action<string> Result;

        public string ProcessNPR(SyndicationFeed syndicationFeed)
        {
            string dirPath = Path.Combine(_baseDirPath, syndicationFeed.Title.Text.ToValidDirName());
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            return dirPath;
        }
    }
}