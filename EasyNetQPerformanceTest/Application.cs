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
                if (_applicationOptions.Publish)
                {
                    var stats = new PublishRunner(_applicationOptions).Run();

                    Console.WriteLine($"Publish total count = {stats.TotalHits}");
                    Console.WriteLine($"Publish per second = {stats.HitsPerSecond}");
                    Console.WriteLine($"Min hit duration = {TimeSpan.FromTicks(stats.MinDurationTicks)}");
                    Console.WriteLine($"Max hit duration = {TimeSpan.FromTicks(stats.MaxDurationTicks)}");
                    Console.WriteLine($"Test duration = {TimeSpan.FromMilliseconds(stats.TestDurationMilliseconds)}");
                }
                else if (_applicationOptions.Subscribe)
                {
                    var stats = new SubscribeRunner(_applicationOptions).Run();

                    Console.WriteLine($"Consume total count = {stats.TotalHits}");
                    Console.WriteLine($"Consume per second = {stats.HitsPerSecond}");
                    Console.WriteLine($"Test duration = {TimeSpan.FromMilliseconds(stats.TestDurationMilliseconds)}");
                }

                return 0;
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