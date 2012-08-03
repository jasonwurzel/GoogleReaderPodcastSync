using System;
using GoogleReaderAPI2;

namespace Flows.Npr.DownloadPodcastsFromReaderFlows
{
    public class GetReader
    {
        private string _email;
        private Reader _reader;

        public GetReader(string email, Reader reader)
        {
            _email = email;
            _reader = reader;
        }
        public void Process(string password)
        {
            _reader = Reader.CreateReader(_email, password, "scroll") as Reader;
            Result(_reader);
        }

        public event Action<Reader> Result;
    }
}