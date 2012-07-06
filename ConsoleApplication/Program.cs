using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Flows;
using GoogleReaderAPI2;
using Tools;
using Tools.DnpExtensions;
using npantarhei.runtime.patterns.operations;

namespace ConsoleApplication
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string email = ConfigurationManager.AppSettings["GoogleAccount"];
            string baseDirPath = String.IsNullOrEmpty(ConfigurationManager.AppSettings["BasePath"]) ? @"c:\temp\" : ConfigurationManager.AppSettings["BasePath"];
            bool deleteOlderFiles = bool.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["DeleteOlderFiles"]) ? "false" : ConfigurationManager.AppSettings["DeleteOlderFiles"]);
            var listenSubscriptions = "Listen Subscriptions";
            int podCastsdownloaded = 0;
            string dateFormat = "yyyyMMddTHHmmss";
            int getFilesFromTheLastXDays =
                int.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["GetFilesFromTheLastXDays"]) ? "3" : ConfigurationManager.AppSettings["GetFilesFromTheLastXDays"]);
            Reader reader = null;

            // DeleteOlderFiles
            // GetFilesFromTheLastXDays

            Console.WriteLine("Enter password");
            var password = Console.ReadLine();

            reader = Reader.CreateReader(email, password, "scroll") as Reader;

            DownloadPodcastsFromReader downloadPodcastsFromReader = new DownloadPodcastsFromReader(listenSubscriptions, baseDirPath, dateFormat, deleteOlderFiles, reader, getFilesFromTheLastXDays);
            downloadPodcastsFromReader.Process();

            Console.WriteLine("************* Fertig **************");
            Console.ReadLine();
        }
    }
}
