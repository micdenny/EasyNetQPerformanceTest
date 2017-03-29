using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private long _publishCount = 0;
        private long _minDuration = long.MaxValue;
        private long _maxDuration = long.MinValue;

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
                            tasks.Add(ProfileRequestAsync(bus));
                        }
                        Task.WaitAll(tasks.ToArray());
                    }
                    else if (_applicationOptions.Concurrency == 0)
                    {
                        Func<Task> profile = async () =>
                        {
                            for (int i = 0; i < _applicationOptions.HitsCount; i++)
                            {
                                await ProfileRequestAsync(bus);
                            }
                        };
                        profile().Wait();
                    }
                    else if (_applicationOptions.Concurrency > 0)
                    {
                        int counter = 0;
                        Func<Task> profile = async () =>
                        {
                            while (Interlocked.Increment(ref counter) <= _applicationOptions.HitsCount)
                            {
                                await ProfileRequestAsync(bus);
                            }
                        };
                        var tasks = new List<Task>(_applicationOptions.Concurrency);
                        for (int i = 0; i < _applicationOptions.Concurrency; i++)
                        {
                            tasks.Add(profile());
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
                            tasks.Add(ProfileRequestAsync(bus));
                        }
                        Task.WaitAll(tasks.ToArray());
                    }
                    else if (_applicationOptions.Concurrency == 0)
                    {
                        Func<Task> profile = async () =>
                        {
                            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_applicationOptions.DurationInSeconds));
                            while (!cts.Token.IsCancellationRequested)
                            {
                                await ProfileRequestAsync(bus);
                            }
                        };
                        profile().Wait();
                    }
                    else if (_applicationOptions.Concurrency > 0)
                    {
                        Func<CancellationToken, Task> profile = async (cancellationToken) =>
                        {
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                await ProfileRequestAsync(bus);
                            }
                        };
                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_applicationOptions.DurationInSeconds));
                        var tasks = new List<Task>(_applicationOptions.Concurrency);
                        for (int i = 0; i < _applicationOptions.Concurrency; i++)
                        {
                            tasks.Add(profile(cts.Token));
                        }
                        Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(_applicationOptions.DurationInSeconds));
                    }
                }
                sw.Stop();
                var hitsPerSecond = (int)(_publishCount / sw.Elapsed.TotalSeconds);

                responder.Dispose();

                // clean up
                if (_applicationOptions.CleanAfterTestDone)
                {
                    var queueName = bus.Advanced.Conventions.RpcRoutingKeyNamingConvention(typeof(PerfRequest));
                    bus.Advanced.QueueDelete(new Queue(queueName, false));
                }

                return new Stats(_publishCount, hitsPerSecond, sw.ElapsedMilliseconds, _minDuration, _maxDuration);
            }
            finally
            {
                bus.Dispose();
            }
        }

        public async Task<Stopwatch> ProfileRequestAsync(IBus bus)
        {
            var swh = Stopwatch.StartNew();

            await bus.RequestAsync<PerfRequest, PerfResponse>(new PerfRequest());

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
