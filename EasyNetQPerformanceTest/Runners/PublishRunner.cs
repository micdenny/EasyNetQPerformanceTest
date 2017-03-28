using System;
using System.Diagnostics;
using System.Threading;
using EasyNetQ;
using EasyNetQ.Topology;
using EasyNetQPerformanceTest.Contract;

namespace EasyNetQPerformanceTest.Runners
{
    public class PublishRunner : IRunner
    {
        private readonly ApplicationOptions _applicationOptions;

        public PublishRunner(ApplicationOptions applicationOptions)
        {
            _applicationOptions = applicationOptions;
        }

        public Stats Run()
        {
            const string subscriptionId = "PerfTest";

            var bus = RabbitHutch.CreateBus("host=127.0.0.1;timeout=0");
            try
            {
                // startup
                if (_applicationOptions.UseQueue)
                {
                    // this will simply create and bind a queue with the default conventions
                    // we call the dispose because we won't consume from it, we just test publishing
                    bus.Subscribe<PerfMessage>(subscriptionId, message => { }).Dispose();
                }

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

                        bus.Publish(new PerfMessage());

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

                        bus.Publish(new PerfMessage());

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

                // clean up
                if (_applicationOptions.CleanAfterTestDone)
                {
                    var queueName = bus.Advanced.Conventions.QueueNamingConvention(typeof(PerfMessage), subscriptionId);
                    bus.Advanced.QueueDelete(new Queue(queueName, false));

                    var exchangeName = bus.Advanced.Conventions.ExchangeNamingConvention(typeof(PerfMessage));
                    bus.Advanced.ExchangeDelete(new Exchange(exchangeName));
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