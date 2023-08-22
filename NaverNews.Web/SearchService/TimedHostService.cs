namespace NaverNews.Web
{
    internal abstract class TimedService
    {
        protected readonly ILogger<TimedService> _logger;
        private readonly TimeSpan _frequency;
        private int _executionCount;

        public TimedService(TimeSpan frequency, ILogger<TimedService> logger)
        {
            _frequency = frequency;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
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

        protected abstract Task DoWork();
    }
}