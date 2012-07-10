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
using WpfApplication;
using npantarhei.runtime.patterns.operations;

namespace ConsoleApplication
{
    internal class Program
    {
        private static Reader _reader;
        private static string _email;
        private static string _listenSubscriptions;
        private static string _baseDirPath;
        private static string _dateFormat;
        private static bool _deleteOlderFiles;
        private static int _getFilesFromTheLastXDays;
        private static MainWindow _window;

        [STAThread]
        private static void Main(string[] args)
        {
            _email = ConfigurationManager.AppSettings["GoogleAccount"];
            _baseDirPath = String.IsNullOrEmpty(ConfigurationManager.AppSettings["BasePath"]) ? @"c:\temp\" : ConfigurationManager.AppSettings["BasePath"];
            _deleteOlderFiles = bool.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["DeleteOlderFiles"]) ? "false" : ConfigurationManager.AppSettings["DeleteOlderFiles"]);
            _listenSubscriptions = "Listen Subscriptions";
            _dateFormat = "yyyyMMddTHHmmss";
            _getFilesFromTheLastXDays =
                int.Parse(String.IsNullOrEmpty(ConfigurationManager.AppSettings["GetFilesFromTheLastXDays"]) ? "3" : ConfigurationManager.AppSettings["GetFilesFromTheLastXDays"]);
            _reader = null;

            MainWindow window = new MainWindow();
            _window = window;
            window.PasswordReceived += WindowOnPasswordReceived;
            window.ShowDialog();
        }

        private static void WindowOnPasswordReceived(string password)
        {
            _reader = Reader.CreateReader(_email, password, "scroll") as Reader;

            DownloadPodcastsFromReader downloadPodcastsFromReader = new DownloadPodcastsFromReader(_listenSubscriptions, _baseDirPath, _dateFormat, _deleteOlderFiles, _reader, _getFilesFromTheLastXDays, _window);
            downloadPodcastsFromReader.Process();

            Console.WriteLine("************* Fertig **************");
            Console.ReadLine();

            Thread.Sleep(5000);
            _window.Close();
        }
    }
}
