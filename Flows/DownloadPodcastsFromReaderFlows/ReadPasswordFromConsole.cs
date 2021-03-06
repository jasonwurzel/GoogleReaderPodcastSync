using System;

namespace Flows.DownloadPodcastsFromReaderFlows
{
    internal class ReadPasswordFromConsole
    {
        public void Process()
        {
            Console.WriteLine("Enter password");
            var password = Console.ReadLine();

            Result(password);
        }

        public event Action<string> Result;
    }
}