using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class ScatterStream<T>
    {
        private Task<T>[] _tasks;
        private int _taskCount = 2;
        private int _switch = 0;

        public ScatterStream()
        {
            ThreadPool.SetMaxThreads(2, 2);
        }
        public void Process(T input )
        {
            if (_switch == 0)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(process1), input);
                _switch = 1;
            }
            else if (_switch == 1)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(process2), input);
                _switch = 0;
            }
        }

        private void process1(Object input)
        {
            if (Output1 != null)
                Output1((T)input);
        }

        private void process2(Object input)
        {
            if (Output2 != null)
                Output2((T)input);
        }


        public event Action<T> Output1;
        public event Action<T> Output2;
    }
}