using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleApplication;
using GoogleReaderAPI2;
using Tools;

namespace Flows
{
    public class DownloadPodcastsFromReader
    {
        private string _listenSubscriptions;
        private string _baseDirPath;
        private string _dateFormat;
        private bool _deleteOlderFiles;
        private Reader _reader;
        private int _getFilesFromTheLastXDays;

        public DownloadPodcastsFromReader(string listenSubscriptions, string baseDirPath, string dateFormat, bool deleteOlderFiles, Reader reader, int getFilesFromTheLastXDays)
        {
            _listenSubscriptions = listenSubscriptions;
            _baseDirPath = baseDirPath;
            _dateFormat = dateFormat;
            _deleteOlderFiles = deleteOlderFiles;
            _reader = reader;
            _getFilesFromTheLastXDays = getFilesFromTheLastXDays;
        }

        public void Process()
        {
            GetFeedsWithGivenLabel getFeedsWithGivenLabel = new GetFeedsWithGivenLabel(_listenSubscriptions);
            EnsureDownloadDirectoryForFeed ensureDownloadDirectoryForFeed = new EnsureDownloadDirectoryForFeed(_baseDirPath);
            ClearDirectoryOfFiles clearDirectoryOfFilesOlderThan = new ClearDirectoryOfFiles(_dateFormat, _deleteOlderFiles, _getFilesFromTheLastXDays);
            GetPodcastLinksFromFeed getPodcastLinksFromFeed = new GetPodcastLinksFromFeed(_reader, _getFilesFromTheLastXDays);
            GetRemoteAndLocalAddress getRemoteAndLocalAddress = new GetRemoteAndLocalAddress(link => link.PublishDate.ToString(_dateFormat) + "_" + link.Title.ToValidFileName() + ".mp3");
            FilterExistingFiles filterExistingFiles = new FilterExistingFiles();
            DownloadFile downloadFile1 = new DownloadFile();

            //ScatterStream<RemoteAndLocalAddress> scatterStream = new ScatterStream<RemoteAndLocalAddress>();

            getFeedsWithGivenLabel.Result += urlAndFeed => ensureDownloadDirectoryForFeed.Process(urlAndFeed.Feed);
            getFeedsWithGivenLabel.Result += urlAndFeed => getPodcastLinksFromFeed.Process(urlAndFeed.Url);

            ensureDownloadDirectoryForFeed.Result += clearDirectoryOfFilesOlderThan.Process;
            clearDirectoryOfFilesOlderThan.Result += getRemoteAndLocalAddress.ProcessDirPath;
            getPodcastLinksFromFeed.Result += getRemoteAndLocalAddress.ProcessPodcastLinkInformation;
            getRemoteAndLocalAddress.Result += filterExistingFiles.Process;
            filterExistingFiles.ResultForNotExistingFile += downloadFile1.Process;

            getFeedsWithGivenLabel.Process(_reader);
        }

        public event Action Result;
    }
}
