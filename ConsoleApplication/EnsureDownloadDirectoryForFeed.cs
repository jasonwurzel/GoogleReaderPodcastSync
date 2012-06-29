using System.IO;
using System.ServiceModel.Syndication;
using Tools;

static internal class EnsureDownloadDirectoryForFeed
{
    public static string Process(SyndicationFeed syndicationFeed, string baseDirPath)
    {
        string dirPath = Path.Combine(baseDirPath, syndicationFeed.Title.Text.ToValidDirName());
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        return dirPath;
    }
}