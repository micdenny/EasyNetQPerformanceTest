using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using EasyNetQPerformanceTest.Contract;

namespace EasyNetQPerformanceTest.Runners
{

    public class SubscribeRunner : IRunner
    {
        private readonly ApplicationOptions _applicationOptions;

        public SubscribeRunner(ApplicationOptions applicationOptions)
        {
            _applicationOptions = applicationOptions;
        }

        public Stats Run()
        {
            const string subscriptionId = "PerfTest";

            var bus = RabbitHutch.CreateBus("host=127.0.0.1;timeout=0");

            // startup
            bus.Subscribe<PerfMessage>(subscriptionId, message => { }).Dispose();
            for (int i = 0; i < _applicationOptions.MessageCount; i++)
            {
                bus.Publish(new PerfMessage());
            }

            // -------------------
            //     PERF TEST
            // -------------------
            var sw = Stopwatch.StartNew();
            if (_applicationOptions.MessageCount > 0)
            {
                var countdown = new CountdownEvent(_applicationOptions.MessageCount);

                var subscription = bus.Subscribe<PerfMessage>(subscriptionId, message =>
                {
                    countdown.Signal();
                });

                countdown.Wait();

                subscription.Dispose();
            }
            sw.Stop();
            var hitsPerSecond = (int)(_applicationOptions.MessageCount / sw.Elapsed.TotalSeconds);

            // clean up
            if (_applicationOptions.CleanAfterTestDone)
            {
                var queueName = bus.Advanced.Conventions.QueueNamingConvention(typeof(PerfMessage), subscriptionId);
                bus.Advanced.QueueDelete(new Queue(queueName, false));

                var exchangeName = bus.Advanced.Conventions.ExchangeNamingConvention(typeof(PerfMessage));
                bus.Advanced.ExchangeDelete(new Exchange(exchangeName));
            }

            bus.Dispose();

            return new Stats(_applicationOptions.MessageCount, hitsPerSecond, sw.ElapsedMilliseconds, 0, 0);
        }
    }
}