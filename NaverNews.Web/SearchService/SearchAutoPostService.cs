using NaverNews.Core;

namespace NaverNews.Web
{
    internal class SearchAutoPostService : TimedHostedService
    {
        private readonly ArticleService _articleService;

        public SearchAutoPostService(ArticleService articleService, ILogger<TimedHostedService> logger)
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