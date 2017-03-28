using System;
using System.Diagnostics;
using EasyNetQPerformanceTest.Runners;

namespace EasyNetQPerformanceTest
{
    public interface IApplication
    {
        int Run();
    }

    public class Application : IApplication
    {
        private ApplicationOptions _applicationOptions;

        public Application(ApplicationOptions applicationOptions)
        {
            _applicationOptions = applicationOptions;
        }

        public int Run()
        {
            try
            {
                IRunner runner = null;

                if (_applicationOptions.Publish)
                {
                    runner = new PublishRunner(_applicationOptions);
                }
                else if (_applicationOptions.Subscribe)
                {
                    runner = new SubscribeRunner(_applicationOptions);
                }
                else if (_applicationOptions.Rpc)
                {
                    runner = new RpcRunner(_applicationOptions);
                }
                else if (_applicationOptions.RpcAsync)
                {
                    runner = new RpcAsyncRunner(_applicationOptions);
                }

                if (runner != null)
                {
                    Stats stats = runner.Run();

                    Console.WriteLine($"Total hits = {stats.TotalHits}");
                    Console.WriteLine($"Hits per second = {stats.HitsPerSecond}");
                    Console.WriteLine($"Min hit duration = {TimeSpan.FromTicks(stats.MinDurationTicks)}");
                    Console.WriteLine($"Max hit duration = {TimeSpan.FromTicks(stats.MaxDurationTicks)}");
                    Console.WriteLine($"Test duration = {TimeSpan.FromMilliseconds(stats.TestDurationMilliseconds)}");
                }

                return 0;
            }
            catch (AggregateException ex)
            {
                var iex = ex.InnerException;
                Trace.TraceError(iex.ToString());
                Console.Error.WriteLine(iex.Message);
                return iex.HResult;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                Console.Error.WriteLine(ex.Message);
                return ex.HResult;
            }
        }
    }
}