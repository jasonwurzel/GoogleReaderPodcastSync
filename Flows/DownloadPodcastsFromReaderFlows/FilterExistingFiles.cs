using System;
using System.Collections.Generic;
using System.IO;

namespace Flows.DownloadPodcastsFromReaderFlows
{
    public class FilterExistingFiles
    {
        public void Process(IEnumerable<RemoteAndLocalAddress> address)
        {
            foreach (var remoteAndLocalAddress in address)
            {
                if (!File.Exists(remoteAndLocalAddress.LocalAddress))
                {
                    if (ResultForNotExistingFile != null)
                        ResultForNotExistingFile(remoteAndLocalAddress);
                }
                else
                {
                    if (ResultForExistingFile != null)
                        ResultForExistingFile(remoteAndLocalAddress);
                }
            }
        }

        public event Action<RemoteAndLocalAddress> ResultForNotExistingFile;
        public event Action<RemoteAndLocalAddress> ResultForExistingFile;
    }
}