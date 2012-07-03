using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApplication
{
    public class Scatter<T>
    {
        public void Process(IEnumerable<T> input )
        {
            var enumerator = input.GetEnumerator();
            var thread1 = new Thread(() => Output1(GenerateOutput(enumerator)));
            var thread2 = new Thread(() => Output2(GenerateOutput(enumerator)));
            thread1.Start();
            thread2.Start();
        }

        private IEnumerable<T> GenerateOutput(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        public event Action<IEnumerable<T>> Output1;
        public event Action<IEnumerable<T>> Output2;
    }
}