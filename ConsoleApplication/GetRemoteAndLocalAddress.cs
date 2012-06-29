using System;
using System.IO;
using Tools;

namespace ConsoleApplication
{
    static internal class GetRemoteAndLocalAddress
    {
        public static RemoteAndLocalAddress Process(string dirPath, PodcastLinkInformation podcastLink)
        {
            string remoteAdress = podcastLink.FileAddress;
            Console.WriteLine(remoteAdress);
            string localFileName = podcastLink.PublishDate.ToString("yyyyMMddTHHmmss") + "_" + podcastLink.Title.ToValidFileName() + ".mp3";
            var localFilePath = Path.Combine(dirPath, localFileName);
            Console.WriteLine(localFileName);
            return new RemoteAndLocalAddress(remoteAdress, localFilePath);
        }
    }

    internal class RemoteAndLocalAddress
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