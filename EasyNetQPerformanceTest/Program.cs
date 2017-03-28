using Microsoft.Extensions.CommandLineUtils;

namespace EasyNetQPerformanceTest
{
    public class Program
    {
        private const int ParametersError = -999;

        public static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);

            CommandOption publish = commandLineApplication.Option(
                "-p|--publish",
                "Publish => bus.Publish(new PerfMessage())",
                CommandOptionType.NoValue);

            CommandOption subscribe = commandLineApplication.Option(
               "-s|--subscribe",
               "Subscribe => bus.Subscribe<PerfMessage>(\"PerfTest\", message => { ... })",
               CommandOptionType.NoValue);

            CommandOption rpc = commandLineApplication.Option(
               "-r|--rpc",
               "RPC => bus.Request+Respond<PerfRequest, PerfResponse>( ... )",
               CommandOptionType.NoValue);

            CommandOption rpcAsync = commandLineApplication.Option(
               "-R|--rpc-async",
               "RPC async => bus.RequestAsync+RespondAsync<PerfRequest, PerfResponse>( ... )",
               CommandOptionType.NoValue);

            CommandOption queue = commandLineApplication.Option(
                "-q|--use-queue",
                "When set, the test will run against a queue binded to an exchange.",
                CommandOptionType.NoValue);

            CommandOption time = commandLineApplication.Option(
                "-t|--time",
                "The duration of the test in seconds. Default is 30 seconds.",
                CommandOptionType.SingleValue);

            CommandOption count = commandLineApplication.Option(
                "-c|--count",
                "The number of hits to do.",
                CommandOptionType.SingleValue);

            CommandOption messageCount = commandLineApplication.Option(
                "-C|--message-count",
                "The number of messages to fill a pre-declared queue.",
                CommandOptionType.SingleValue);

            CommandOption disableCleanup = commandLineApplication.Option(
                "--no-cleanup",
                "Disable the cleanup after the test is done.",
                CommandOptionType.NoValue);

            commandLineApplication.HelpOption("-?|-h|--help");

            commandLineApplication.OnExecute(() =>
            {
                var options = new ApplicationOptions();

                if (publish.HasValue())
                {
                    options.Publish = publish.HasValue();
                }

                if (subscribe.HasValue())
                {
                    options.Subscribe = true;
                }

                if (rpc.HasValue())
                {
                    options.Rpc = true;
                }

                if (rpcAsync.HasValue())
                {
                    options.RpcAsync = true;
                }

                if (queue.HasValue())
                {
                    options.UseQueue = true;
                }

                if (count.HasValue())
                {
                    int value;
                    if (int.TryParse(count.Value(), out value))
                    {
                        options.HitsCount = value;
                    }
                    else
                    {
                        commandLineApplication.Error.WriteLine($"-{count.ShortName}|--{count.LongName} must be a duration in seconds.");
                        return ParametersError;
                    }
                }

                if (messageCount.HasValue())
                {
                    int value;
                    if (int.TryParse(messageCount.Value(), out value))
                    {
                        options.MessageCount = value;
                    }
                    else
                    {
                        commandLineApplication.Error.WriteLine($"-{messageCount.ShortName}|--{messageCount.LongName} must be a duration in seconds.");
                        return ParametersError;
                    }
                }

                if (time.HasValue())
                {
                    int value;
                    if (int.TryParse(time.Value(), out value))
                    {
                        options.DurationInSeconds = value;
                    }
                    else
                    {
                        commandLineApplication.Error.WriteLine($"-{time.ShortName}|--{time.LongName} must be a duration in seconds.");
                        return ParametersError;
                    }
                }

                if (disableCleanup.HasValue())
                {
                    options.CleanAfterTestDone = false;
                }

                var app = new Application(options);
                return app.Run();
            });

            if (args.Length > 0)
            {
                commandLineApplication.Execute(args);
            }
            else
            {
                commandLineApplication.ShowHelp();
            }
        }
    }
}
