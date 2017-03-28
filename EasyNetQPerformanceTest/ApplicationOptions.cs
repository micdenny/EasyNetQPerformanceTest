namespace EasyNetQPerformanceTest
{
    public class ApplicationOptions
    {
        public bool Publish { get; set; } = false;
        public bool Subscribe { get; set; } = false;
        public bool Rpc { get; set; } = false;
        public bool UseQueue { get; set; } = false;
        public int DurationInSeconds { get; set; } = 30;
        public int HitsCount { get; set; } = 0;
        public int MessageCount { get; set; } = 0;
        public bool CleanAfterTestDone { get; set; } = true;
    }
}