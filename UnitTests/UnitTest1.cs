using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using npantarhei.runtime;
using npantarhei.runtime.contract;
using npantarhei.runtime.messagetypes;

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
                frc.AddStream(".in", "Add_Ebc1.process");
                frc.AddStream("Add_Ebc1.result", "join.in0");
                frc.AddStream("Add_Ebc1.result2", "join.in1");
                frc.AddStream("join", ".out");
                
                frc.AddAutoResetJoin<int, int>("join");
                frc.AddEventBasedComponent("Add_Ebc1", new Add_Ebc());
                
                fr.Configure(frc);

                fr.Process(new Message(".in", 1));

                Tuple<int, int> erg = null;

                fr.WaitForResult(_ => erg = (Tuple<int, int>) _.Data);

                erg.Should().Be(new Tuple<int, int>(2, 3));

            }
        }

        public class Add_Ebc
        {
            public void Process(int i)
            {
                Result(i + 1);
                Result2(i + 2);
            }

            public event Action<int> Result = _ => {};
            public event Action<int> Result2 = _ => {};
        }
    }


}
