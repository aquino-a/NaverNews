namespace NaverNews.Web
{
    public abstract class TimedHostedService : BackgroundService
    {
        protected readonly ILogger<TimedHostedService> _logger;
        private readonly TimeSpan _frequency;
        private int _executionCount;

        public TimedHostedService(TimeSpan frequency, ILogger<TimedHostedService> logger)
        {
            _frequency = frequency;
            _logger = logger;
        }

        // Could also be a async method, that can be awaited in ExecuteAsync above
        protected virtual async Task DoWork()
        {
            int count = Interlocked.Increment(ref _executionCount);

            _logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            // When the timer should have no due-time, then do the work once now.
            await DoWork();

            using PeriodicTimer timer = new(_frequency);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWork();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }
    }
}