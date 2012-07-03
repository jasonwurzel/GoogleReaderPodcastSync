using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleApplication
{
    public class FilterExistingFiles
    {
        public void Process(IEnumerable<RemoteAndLocalAddress> address)
        {
            List<RemoteAndLocalAddress> notExistingFiles = new List<RemoteAndLocalAddress>();

            foreach (var remoteAndLocalAddress in address)
            {
                if (!File.Exists(remoteAndLocalAddress.LocalAddress))
                    notExistingFiles.Add(remoteAndLocalAddress);
                else
                {
                    if (ResultForExistingFile != null)
                        ResultForExistingFile(remoteAndLocalAddress);
                }
            }

            ResultForNotExistingFile(notExistingFiles);


        }

        public event Action<IEnumerable<RemoteAndLocalAddress>> ResultForNotExistingFile;
        public event Action<RemoteAndLocalAddress> ResultForExistingFile;
    }
}