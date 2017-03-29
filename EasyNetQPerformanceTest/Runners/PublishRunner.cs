using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using EasyNetQPerformanceTest.Contract;

namespace EasyNetQPerformanceTest.Runners
{
    public class PublishRunner : IRunner
    {
        private long _publishCount = 0;
        private long _minDuration = long.MaxValue;
        private long _maxDuration = long.MinValue;

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

                // -------------------
                //     PERF TEST
                // -------------------
                var sw = Stopwatch.StartNew();
                if (_applicationOptions.HitsCount > 0)
                {
                    if (_applicationOptions.Concurrency == -1)
                    {
                        var tasks = new List<Task>(_applicationOptions.HitsCount);
                        for (int i = 0; i < _applicationOptions.HitsCount; i++)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                ProfilePublish(bus);
                            }));
                        }
                        Task.WaitAll(tasks.ToArray());
                    }
                    else if (_applicationOptions.Concurrency == 0)
                    {
                        var swh = new Stopwatch();
                        for (int i = 0; i < _applicationOptions.HitsCount; i++)
                        {
                            ProfilePublish(bus);
                        }
                    }
                    else if (_applicationOptions.Concurrency > 0)
                    {
                        int counter = 0;
                        var tasks = new List<Task>(_applicationOptions.Concurrency);
                        for (int i = 0; i < _applicationOptions.Concurrency; i++)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                while (Interlocked.Increment(ref counter) <= _applicationOptions.HitsCount)
                                {
                                    ProfilePublish(bus);
                                }
                            }));
                        }
                        Task.WaitAll(tasks.ToArray());
                    }
                }
                else if (_applicationOptions.DurationInSeconds > 0)
                {
                    if (_applicationOptions.Concurrency == -1)
                    {
                        var tasks = new List<Task>();
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_applicationOptions.DurationInSeconds));
                        while (!cts.Token.IsCancellationRequested)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                ProfilePublish(bus);
                            }));
                        }
                        Task.WaitAll(tasks.ToArray());
                    }
                    else if (_applicationOptions.Concurrency == 0)
                    {
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_applicationOptions.DurationInSeconds));
                        var swh = new Stopwatch();
                        while (!cts.Token.IsCancellationRequested)
                        {
                            ProfilePublish(bus);
                        }
                    }
                    else if (_applicationOptions.Concurrency > 0)
                    {
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_applicationOptions.DurationInSeconds));
                        var tasks = new List<Task>(_applicationOptions.Concurrency);
                        for (int i = 0; i < _applicationOptions.Concurrency; i++)
                        {
                            tasks.Add(Task.Factory.StartNew(() =>
                            {
                                while (!cts.Token.IsCancellationRequested)
                                {
                                    ProfilePublish(bus);
                                }
                            }, TaskCreationOptions.LongRunning));
                        }
                        Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(_applicationOptions.DurationInSeconds));
                    }
                }
                sw.Stop();
                var hitsPerSecond = (int)(_publishCount / sw.Elapsed.TotalSeconds);

                // clean up
                if (_applicationOptions.CleanAfterTestDone)
                {
                    var queueName = bus.Advanced.Conventions.QueueNamingConvention(typeof(PerfMessage), subscriptionId);
                    bus.Advanced.QueueDelete(new Queue(queueName, false));

                    var exchangeName = bus.Advanced.Conventions.ExchangeNamingConvention(typeof(PerfMessage));
                    bus.Advanced.ExchangeDelete(new Exchange(exchangeName));
                }

                return new Stats(_publishCount, hitsPerSecond, sw.ElapsedMilliseconds, _minDuration, _maxDuration);
            }
            finally
            {
                bus.Dispose();
            }
        }

        public Stopwatch ProfilePublish(IBus bus)
        {
            var swh = Stopwatch.StartNew();

            bus.Publish(new PerfMessage());

            swh.Stop();

            Interlocked.Increment(ref _publishCount);

            if (swh.ElapsedTicks < _minDuration)
            {
                Interlocked.Exchange(ref _minDuration, swh.ElapsedTicks);
            }
            if (swh.ElapsedTicks > _maxDuration)
            {
                Interlocked.Exchange(ref _maxDuration, swh.ElapsedTicks);
            }

            return swh;
        }
    }
}