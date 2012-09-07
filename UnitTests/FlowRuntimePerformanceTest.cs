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
	public class FlowRuntimePerformanceTest
    {
		/// <summary>
		/// Einfacher Test, um ein Gefühl für den Overhead der FR zu kriegen
		/// </summary>
		[Test]
        public void TestComparePerformance()
        {
			Stopwatch stopwatch = new Stopwatch();
			
			stopwatch.Start();
			int upper = 1000000000;
			int erg = 0;


			//for (int i = 0; i < upper; i++)
				erg = ++erg;
			stopwatch.Stop();

			var elapsed1 = stopwatch.ElapsedMilliseconds;

			long elapsed2 = 0;

			using (var fr = new FlowRuntime())
            {
                var frc = new FlowRuntimeConfiguration();
                frc.AddStreamsFrom(@"
                                    /
                                    .in, inc
                                    inc, .out
                                    ");

	            frc.AddFunc<int, int>("inc", inc);
                
                fr.Configure(frc);

                stopwatch = new Stopwatch();
				erg = 0;
				stopwatch.Start();
				//for (int i = 0; i < upper; i++)
				//{
					fr.Process(new Message(".in", erg));
					fr.WaitForResult(_ => erg = (int)_.Data);
				//}
                stopwatch.Stop();
	            elapsed2 = stopwatch.ElapsedMilliseconds;
            }
        }

	    private int inc(int i)
	    {
		    return ++i;
	    }

        
    }


}
