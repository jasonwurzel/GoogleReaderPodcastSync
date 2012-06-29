using System;
using System.IO;
using System.ServiceModel.Syndication;
using Tools;

namespace ConsoleApplication
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
    }
}