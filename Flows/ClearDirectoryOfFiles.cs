using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ConsoleApplication
{
    public class ClearDirectoryOfFiles
    {
        private string _dateFormat;
        private double _deleteFilesOlderThanXDays;

        public ClearDirectoryOfFiles(string dateFormat, double deleteFilesOlderThanXDays)
        {
            _dateFormat = dateFormat;
            _deleteFilesOlderThanXDays = deleteFilesOlderThanXDays;
        }

        public void Process(string dirPath)
        {
            var list = new List<string>();
            foreach (var filePath in Directory.GetFiles(dirPath))
            {
                var file = Path.GetFileName(filePath);
                var dateString = file.Substring(0, _dateFormat.Length);
                DateTime dt = DateTime.ParseExact(dateString, _dateFormat, CultureInfo.InvariantCulture);
                if (dt < DateTime.Now.AddDays(-_deleteFilesOlderThanXDays))
                    File.Delete(file);
            }

            // einfach durchreichen
            Result(dirPath);
        }

        public event Action<string> Result;
    }
}