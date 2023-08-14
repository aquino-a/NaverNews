using NaverNews.Core;

namespace NaverNews.Web
{
    internal class SearchService : TimedHostedService
    {
        private readonly ArticleService _articleService;

        public SearchService(ArticleService articleService, ILogger<TimedHostedService> logger)
            : base(TimeSpan.FromHours(1), logger)
        {
            _articleService = articleService;
        }

        protected override async Task DoWork()
        {
            try
            {
                await _articleService.SearchArticles(NewsType.Society, 20);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Problem searching articles.");
            }
        }
    }
}