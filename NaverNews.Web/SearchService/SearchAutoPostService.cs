using NaverNews.Core;

namespace NaverNews.Web
{
    internal class SearchAutoPostService : TimedHostedService
    {
        private readonly IArticleService _articleService;

        public SearchAutoPostService(IArticleService articleService, ILogger<TimedHostedService> logger)
            : base(TimeSpan.FromHours(1), logger)
        {
            _articleService = articleService;
        }

        protected override async Task DoWork()
        {
            try
            {
                await _articleService.AutoPost();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Problem autoposting articles.");
            }
        }
    }
}