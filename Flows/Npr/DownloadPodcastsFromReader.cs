using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using Flows.Npr.DownloadPodcastsFromReaderFlows;
using GoogleReaderAPI2;
using Tools;
using WpfApplication;
using npantarhei.runtime;

namespace Flows.Npr
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

        #region NPR

        public void ProcessNpr()
        {
            using (var fr = new FlowRuntime())
            {
                var frc = new FlowRuntimeConfiguration();
                var getFeeds = new GetFeedsWithGivenLabel(_reader);

                frc.AddStreamsFrom(@"
                                    /
                                    .in, getFeeds
                                    getFeeds, .out
                                    ");

                frc.AddAction<string, UrlAndFeed>("getFeeds", getFeeds.Process);
            }
        }

        #endregion
    }
}
