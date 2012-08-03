using System;

namespace Flows.Npr.DownloadPodcastsFromReaderFlows
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