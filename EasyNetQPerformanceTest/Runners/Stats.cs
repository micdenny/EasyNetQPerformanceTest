namespace EasyNetQPerformanceTest.Runners
{
    public class Stats
    {
        public long TotalHits { get; set; }
        public int HitsPerSecond { get; set; }
        public long TestDurationMilliseconds { get; set; }
        public long MinDurationTicks { get; set; }
        public long MaxDurationTicks { get; set; }

        public Stats(
            long hits,
            int hitsPerSecond,
            long testDurationMilliseconds,
            long minDurationTicks,
            long maxDurationTicks)
        {
            this.TotalHits = hits;
            this.HitsPerSecond = hitsPerSecond;
            this.TestDurationMilliseconds = testDurationMilliseconds;
            this.MinDurationTicks = minDurationTicks;
            this.MaxDurationTicks = maxDurationTicks;
        }
    }
}