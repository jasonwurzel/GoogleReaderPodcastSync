using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using npantarhei.runtime;
using npantarhei.runtime.messagetypes;
using npantarhei.runtime.patterns;
using npantarhei.runtime.patterns.operations;

namespace UnitTests
{
    /// <summary>
    /// Tests, um die FlowRuntime zu verstehen ^^
    /// </summary>
    [TestFixture]
    public class UnitTest1
    {
		[Test]
        public void TestMethodJoin01()
        {
            using (var fr = new FlowRuntime())
            {
                var frc = new FlowRuntimeConfiguration();
                frc.AddStreamsFrom(@"
                                    /
                                    .in, scatter
                                    scatter.stream, Inc
                                    scatter.count, gather.count
                                    Inc, gather.stream
                                    gather, .out
                                    ");
                
                //frc.AddFunc<int, string>("Inc", convert);
                frc.AddFunc<int, string>("Inc", convert).MakeParallel();

                frc.AddOperation(new Scatter<int>("scatter"));
                frc.AddOperation(new Gather<string>("gather"));
                
                fr.Configure(frc);

                fr.Message += message => Trace.WriteLine(String.Format("Time: {0} Port: {1} Data: {2} Prio: {3}", DateTime.Now.ToString("HHmmss.fff"), message.Port, message.Data, message.Priority));
                //fr.Throttle(500);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                fr.Process(new Message(".in", new []{1, 2, 3, 4}));
                string[] erg = null;

                fr.WaitForResult(_ => erg = (string[]) _.Data);
                stopwatch.Stop();

                erg.Should().BeEquivalentTo(new object[]{"a", "b", "c", "d"});

                var msecs = stopwatch.ElapsedMilliseconds;
                Trace.WriteLine(string.Format("Total: {0}", msecs));
            }
        }

        /// <summary>
        /// Komponente von Hand getestet...
        /// </summary>
		[Test]
        public void TestAufteiler01()
        {
            List<string> ergebnisListe = new List<string>();
            Aufteiler<string> aufteiler = new Aufteiler<string>();
            aufteiler.Receive("data", ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(1);
            aufteiler.Receive("data", ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(2);
            aufteiler.Receive("data", ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(3);
            aufteiler.Receive("data", ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(4);
            // Ab hier sollte gepuffert werden
            aufteiler.Receive("data", ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(4);
            aufteiler.Receive("data", ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(4);

            aufteiler.Signal(ergebnisListe.Add);

            // Puffer geleert
            ergebnisListe.Count.Should().Be(6);
        }

        /// <summary>
        /// Komponente von Hand getestet...
        /// </summary>
		[Test]
        public void TestAufteiler02()
        {
            List<int> ergebnisListe = new List<int>();
            Aufteiler<int> aufteiler = new Aufteiler<int>();
            aufteiler.Receive(1, ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(1);
            aufteiler.Receive(2, ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(2);
            aufteiler.Receive(3, ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(3);
            aufteiler.Receive(4, ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(4);
            ergebnisListe.Should().ContainInOrder(new[] { 1, 2, 3, 4 });
            // Ab hier sollte gepuffert werden
            aufteiler.Receive(5, ergebnisListe.Add);
            aufteiler.Receive(6, ergebnisListe.Add);
            aufteiler.Receive(7, ergebnisListe.Add);
            aufteiler.Receive(8, ergebnisListe.Add);
            aufteiler.Receive(9, ergebnisListe.Add);
            aufteiler.Receive(10, ergebnisListe.Add);
            aufteiler.Receive(11, ergebnisListe.Add);
            aufteiler.Receive(12, ergebnisListe.Add);
            ergebnisListe.Count.Should().Be(4);
            ergebnisListe.Should().ContainInOrder(new[] { 1, 2, 3, 4 });

            aufteiler.Signal(ergebnisListe.Add);
            // Puffer um 4 verkleinert
            ergebnisListe.Count.Should().Be(8);
            ergebnisListe.Should().ContainInOrder(new[] { 1, 2, 3, 4, 5, 6, 7, 8 });

            aufteiler.Signal(ergebnisListe.Add);
            // Puffer um 4 verkleinert
            ergebnisListe.Count.Should().Be(12);
            ergebnisListe.Should().ContainInOrder(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
        }


        /// <summary>
        /// Basic Test zur Integration des Aufteilers in die FlowRuntime
        /// </summary>
		[Test]
        public void TestAufteilerFlow01()
        {
            List<int> ergebnisListe = new List<int>();
            var aufteiler = new Aufteiler<int>();
            var frc = new FlowRuntimeConfiguration();
            frc.AddStreamsFrom(@"
                                /
                                .in, aufteilen
                                aufteilen, .out
                                signal, .out
                                ");
            frc.AddAction<int, int>("aufteilen", aufteiler.Receive);
            frc.AddAction<int>("signal", aufteiler.Signal);

            using (var fr = new FlowRuntime(frc))
            {
                fr.Result += message =>
                    {
                        if (message.Data != null)
                            ergebnisListe.Add((int) message.Data);
                    };

                fr.Process(new Message(".in", 1));
                fr.WaitForResult(_ => {});

                ergebnisListe.Count.Should().Be(1);

                fr.Process(new Message(".in", 2));
                fr.WaitForResult(_ => { });
                ergebnisListe.Count.Should().Be(2);

                fr.Process(new Message(".in", 3));
                fr.WaitForResult(_ => { });
                ergebnisListe.Count.Should().Be(3);

                fr.Process(new Message(".in", 4));
                fr.WaitForResult(_ => { });
                ergebnisListe.Count.Should().Be(4);

                fr.Process(new Message(".in", 5));
                fr.Process(new Message(".in", 6));
                fr.Process(new Message(".in", 7));
                fr.Process(new Message(".in", 8));
                fr.Process(new Message(".in", 9));

                fr.Process(new Message("signal"));
                fr.WaitForResult(_ => { });
                ergebnisListe.Count.Should().Be(8);
            }
        }



        public class Aufteiler<T>
        {
            readonly Queue<T> _buffer = new Queue<T>();
            private const int _itemsToSendTillSignal = 4; // default
            private int _currentSentItems;

            public void Receive(T message, Action<T> continueWith)
            {
                if (_currentSentItems < _itemsToSendTillSignal)
                {
                    _currentSentItems++;
                    continueWith(message);
                }
                else
                {
                    _buffer.Enqueue(message);
                }
            }

            public void Signal(Action<T> continueWith)
            {
                _currentSentItems = 0;
                int newBufferCount = _buffer.Count - _itemsToSendTillSignal > 0 ? _buffer.Count - _itemsToSendTillSignal : 0;
                while (_buffer.Count > newBufferCount)
                {
                    var message = _buffer.Dequeue();
                    continueWith(message);
                }
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
