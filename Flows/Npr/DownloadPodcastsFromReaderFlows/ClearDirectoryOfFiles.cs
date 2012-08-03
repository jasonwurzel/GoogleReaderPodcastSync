using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Flows.Npr.DownloadPodcastsFromReaderFlows
{
    public class ClearDirectoryOfFiles
    {
        private string _dateFormat;
        private bool _deleteOlderFiles;
        private int _getFilesFromTheLastXDays;

        public ClearDirectoryOfFiles(string dateFormat, bool deleteOlderFiles, int getFilesFromTheLastXDays)
        {
            _dateFormat = dateFormat;
            _deleteOlderFiles = deleteOlderFiles;
            _getFilesFromTheLastXDays = getFilesFromTheLastXDays;
        }

        public void Process(string dirPath)
        {
            if (_deleteOlderFiles)
            {
                // Den ganzen Tag betrachten!
                var now = DateTime.Now;
                var dateTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);

                var list = new List<string>();
                foreach (var filePath in Directory.GetFiles(dirPath))
                {
                    var file = Path.GetFileName(filePath);
                    var dateString = file.Substring(0, _dateFormat.Length);
                    DateTime dt = DateTime.ParseExact(dateString, _dateFormat, CultureInfo.InvariantCulture);
                    if (dt < dateTime.AddDays(-_getFilesFromTheLastXDays))
                        File.Delete(filePath);
                }
            }
            // einfach durchreichen
            Result(dirPath);
        }

        public event Action<string> Result;
    }
}