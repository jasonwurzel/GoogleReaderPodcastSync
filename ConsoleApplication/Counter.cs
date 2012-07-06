using System;
using System.Collections.Generic;

namespace ConsoleApplication
{
    public class Counter
    {
        private int _count = 0;
        private int _itemCountToExpect;
        private int _feedCountToExpect;
        private int _actualFeedCount;
        private int _actualItemCount;

        private void CheckCount()
        {
            if (_itemCountToExpect == _actualItemCount)
            {
                PrintCount();
                if (_feedCountToExpect == _actualFeedCount)
                    PrintTotalCount();
            }
        }

        public event Action SignalEnd; 

        public void PrintCount()
        {
            Console.WriteLine("Files downloaded: {0}", _count);
        }

        public void PrintTotalCount()
        {
            Console.WriteLine("************ Total Files downloaded: {0} ***************", _count);
        }

        public void SetItemCountToExpect(int count)
        {
            _actualItemCount = 0;
            _itemCountToExpect = count;
        }

        public void SetFeedCountToExpect(int count)
        {
            _actualFeedCount = 0;
            _feedCountToExpect = count;
        }

        public void CountOneFeed()
        {
            lock (this)
            {
                _actualFeedCount++;
                CheckCount();
            } 
        }

        public void CountOneItem()
        {
            lock (this)
            {
                _actualItemCount++; 
                CheckCount();
            }
        }
    }
}