using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApplication
{
    public class ClearDirectoryOfFiles
    {
        private Func<string, IEnumerable<string>> _findFiles;

        public ClearDirectoryOfFiles(Func<string, IEnumerable<string>> findFilesToDelete)
        {
            _findFiles = findFilesToDelete;
        }
        public void Process(string dirPath)
        {
            List<string> files = new List<string>();

            foreach (var file in _findFiles(dirPath).ToList())
            {
                File.Delete(file);
            }

            // einfach durchreichen
            Result(dirPath);
        }

        public event Action<string> Result;
    }
}