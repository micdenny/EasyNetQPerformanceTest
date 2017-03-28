using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using EasyNetQPerformanceTest.Contract;

namespace EasyNetQPerformanceTest.Runners
{

    public interface IRunner
    {
        Stats Run();
    }
}