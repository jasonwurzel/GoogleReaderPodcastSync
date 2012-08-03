using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using Flows.DownloadPodcastsFromReaderFlows;
using GoogleReaderAPI2;
using Tools;
using WpfApplication;
using npantarhei.runtime;


namespace Flows
{
    public class DownloadPodcastsFromReader
    {
        private string _label;
        private string _baseDirPath;
        private string _dateFormat;
        private bool _deleteOlderFiles;
        private Reader _reader;
        private int _getFilesFromTheLastXDays;
        private MainWindow _window;

        public DownloadPodcastsFromReader(string label, string baseDirPath, string dateFormat, bool deleteOlderFiles, Reader reader, int getFilesFromTheLastXDays, MainWindow window)
        {
            _label = label;
            _baseDirPath = baseDirPath;
            _dateFormat = dateFormat;
            _deleteOlderFiles = deleteOlderFiles;
            _reader = reader;
            _getFilesFromTheLastXDays = getFilesFromTheLastXDays;
            _window = window;
        }

        public void Process()
        {
            GetFeedsWithGivenLabel getFeedsWithGivenLabel = new GetFeedsWithGivenLabel(_label);
            EnsureDownloadDirectoryForFeed ensureDownloadDirectoryForFeed = new EnsureDownloadDirectoryForFeed(_baseDirPath);
            ClearDirectoryOfFiles clearDirectoryOfFilesOlderThan = new ClearDirectoryOfFiles(_dateFormat, _deleteOlderFiles, _getFilesFromTheLastXDays);
            GetPodcastLinksFromFeed getPodcastLinksFromFeed = new GetPodcastLinksFromFeed(_reader, _getFilesFromTheLastXDays);
            GetRemoteAndLocalAddress getRemoteAndLocalAddress = new GetRemoteAndLocalAddress(link => link.PublishDate.ToString(_dateFormat) + "_" + link.Title.ToValidFileName() + ".mp3");
            FilterExistingFiles filterExistingFiles = new FilterExistingFiles();
            DownloadFile downloadFile1 = new DownloadFile();
            int totalDownloads = 0;

            getFeedsWithGivenLabel.Result += urlAndFeed => ensureDownloadDirectoryForFeed.Process(urlAndFeed.Feed);
            getFeedsWithGivenLabel.Result += urlAndFeed => getPodcastLinksFromFeed.Process(urlAndFeed.Url);

            ensureDownloadDirectoryForFeed.Result += clearDirectoryOfFilesOlderThan.Process;
            clearDirectoryOfFilesOlderThan.Result += getRemoteAndLocalAddress.ProcessDirPath;
            getPodcastLinksFromFeed.Result += getRemoteAndLocalAddress.ProcessPodcastLinkInformation;
            getRemoteAndLocalAddress.Result += filterExistingFiles.Process;
            filterExistingFiles.ResultForNotExistingFile += downloadFile1.Process;
            downloadFile1.Result += _ => totalDownloads++;

            getFeedsWithGivenLabel.OnFeedFound += s => _window.ShowTaskbarNotification("Checking Feed", string.Format("Address: {0}", s));
            downloadFile1.OnDownloadStarting += address => _window.ShowTaskbarNotification("Starting Download", string.Format("Remote File: {0}", address.RemoteAddress));
            //downloadFile1.OnDownloadFinished += address => _window.ShowTaskbarNotification("Finished Download", string.Format("Local File: {0}", address.LocalAddress));

            getFeedsWithGivenLabel.Process(_reader);

            _window.ShowTaskbarNotification("Alle Downloads fertig!", string.Format("Anzahl Downloads: {0}", totalDownloads), 1000);
        }
    }
}
