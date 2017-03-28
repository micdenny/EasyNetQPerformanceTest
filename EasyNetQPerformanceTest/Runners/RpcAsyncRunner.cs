using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using EasyNetQPerformanceTest.Contract;

namespace EasyNetQPerformanceTest.Runners
{
    public class RpcAsyncRunner : IRunner
    {
        private readonly ApplicationOptions _applicationOptions;

        public RpcAsyncRunner(ApplicationOptions applicationOptions)
        {
            _applicationOptions = applicationOptions;
        }

        public Stats Run()
        {
            var bus = RabbitHutch.CreateBus($"host=127.0.0.1;timeout={UInt16.MaxValue}"); // infinite timeout (0) doesn't work with RPC really?!
            try
            {
                // startup
                var responder = bus.RespondAsync<PerfRequest, PerfResponse>(request =>
                {
                    return Task.FromResult(new PerfResponse());
                });

                long publishCount = 0;
                long minDuration = long.MaxValue;
                long maxDuration = long.MinValue;

                // -------------------
                //     PERF TEST
                // -------------------
                var sw = Stopwatch.StartNew();
                if (_applicationOptions.HitsCount > 0)
                {
                    var swh = new Stopwatch();
                    for (int i = 0; i < _applicationOptions.HitsCount; i++)
                    {
                        swh.Restart();

                        bus.RequestAsync<PerfRequest, PerfResponse>(new PerfRequest()).Wait();

                        swh.Stop();

                        publishCount++;

                        if (swh.ElapsedTicks < minDuration)
                        {
                            minDuration = swh.ElapsedTicks;
                        }
                        if (swh.ElapsedTicks > maxDuration)
                        {
                            maxDuration = swh.ElapsedTicks;
                        }
                    }
                }
                else if (_applicationOptions.DurationInSeconds > 0)
                {
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_applicationOptions.DurationInSeconds));
                    var swh = new Stopwatch();
                    while (!cts.Token.IsCancellationRequested)
                    {
                        swh.Restart();

                        bus.RequestAsync<PerfRequest, PerfResponse>(new PerfRequest()).Wait();

                        swh.Stop();

                        publishCount++;

                        if (swh.ElapsedTicks < minDuration)
                        {
                            minDuration = swh.ElapsedTicks;
                        }
                        if (swh.ElapsedTicks > maxDuration)
                        {
                            maxDuration = swh.ElapsedTicks;
                        }
                    }
                }
                sw.Stop();
                var hitsPerSecond = (int)(publishCount / sw.Elapsed.TotalSeconds);

                responder.Dispose();

                // clean up
                if (_applicationOptions.CleanAfterTestDone)
                {
                    var queueName = bus.Advanced.Conventions.RpcRoutingKeyNamingConvention(typeof(PerfRequest));
                    bus.Advanced.QueueDelete(new Queue(queueName, false));
                }

                return new Stats(publishCount, hitsPerSecond, sw.ElapsedMilliseconds, minDuration, maxDuration);
            }
            finally
            {
                bus.Dispose();
            }
        }
    }
}
