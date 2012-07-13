using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using npantarhei.runtime;
using npantarhei.runtime.contract;
using npantarhei.runtime.messagetypes;
using npantarhei.runtime.patterns;
using npantarhei.runtime.patterns.operations;

namespace UnitTests
{
    /// <summary>
    /// Tests, um die FlowRuntime zu verstehen ^^
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethodJoin01()
        {
            using (var fr = new FlowRuntime())
            {
                var frc = new FlowRuntimeConfiguration();
                frc.AddStream(".in", "scatter");
                frc.AddStream("scatter.stream", "Inc");
                frc.AddStream("scatter.count", "gather.count");
                frc.AddStream("Inc", "gather.stream");
                frc.AddStream("gather", ".out");
                
                frc.AddFunc<int, string>("Inc", convert);
                //frc.AddFunc<int, string>("Inc", convert).MakeParallel();

                frc.AddOperation(new Scatter<int>("scatter"));
                frc.AddOperation(new Gather<string>("gather"));
                
                fr.Configure(frc);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                fr.Process(new Message(".in", new []{1, 2, 3, 4}));
                string[] erg = null;

                fr.WaitForResult(_ => erg = (string[]) _.Data);
                stopwatch.Stop();

                erg.Should().BeEquivalentTo(new object[]{"a", "b", "c", "d"});

                var secs = stopwatch.ElapsedMilliseconds;
            }
        }

        private string convert(int arg)
        {
            if (arg == 1)
            {
                Thread.Sleep(100);
                return "a";
            }
            if (arg == 2)
            {
                Thread.Sleep(100);
                return "b";
            }
            if (arg == 3)
            {
                Thread.Sleep(100);
                return "c";
            }
            if (arg == 4)
            {
                Thread.Sleep(100);
                return "d";
            }
            
            return "";
        }

        public class Add_Ebc
        {
            public void Process(int i)
            {
                Result(i + 1);
            }

            public event Action<int> Result = _ => {};
        }

        public class AutoResetJoin<T0, T1, T2, T3> : AutoResetJoinBase
        {
            public AutoResetJoin(string name) : base(name, 4, Create_join_tuple) { }

            private static object Create_join_tuple(List<object> joinList)
            {
                return new Tuple<T0, T1, T2, T3>((T0)joinList[0],
                                             (T1)joinList[1],
                                             (T2)joinList[2],
                                             (T3)joinList[3]);
            }
        }
    }


}
