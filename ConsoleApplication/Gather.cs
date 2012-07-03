using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApplication
{
    public class Gather<T>
    {
        private bool resultWasRaised;
        public readonly BlockingCollection<T> queue = new BlockingCollection<T>();
        private bool input1IsEmpty;
        private bool input2IsEmpty;

        public void Input1(IEnumerable<T> input)
        {
            RaiseResultEvent();
            Iterate(input);
            input1IsEmpty = true;
            if (input1IsEmpty && input2IsEmpty)
                queue.CompleteAdding();
        }

        public void Input2(IEnumerable<T> input)
        {
            RaiseResultEvent();
            Iterate(input);
            input2IsEmpty = true;
            if (input1IsEmpty && input2IsEmpty)
                queue.CompleteAdding();

        }

        private void RaiseResultEvent()
        {
            if (resultWasRaised)
                return;

            resultWasRaised = true;
            var thread = new Thread(o => Result(EnumerateTheQueue()));
            thread.Start();
        }

        private IEnumerable<T> EnumerateTheQueue()
        {
            foreach (var item in queue.GetConsumingEnumerable())
                yield return item;
        }

        private void Iterate(IEnumerable<T> input)
        {
            foreach (var t in input)
                queue.Add(t);
        }

        public event Action<IEnumerable<T>> Result;
    }
}