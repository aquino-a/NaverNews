using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NaverNews.Core;

namespace NaverNews.Function
{
    public class AutoPostTimer
    {
        private readonly IArticleService _articleService;
        private readonly ArticleDbContext _articleContext;
        private readonly ILogger _logger;

        public AutoPostTimer(IArticleService articleService, ArticleDbContext articleContext, ILogger<AutoPostTimer> logger)
        {
            _articleService = articleService;
            _articleContext = articleContext;
            _logger = logger;
        }

        [Function("AutoPostTimer")]
        public async Task Run([TimerTrigger("0 0 */4 * * *")] TimerInfo timerInfo)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            try
            {
                await _articleService.AutoPost();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Problem autoposting articles.");
            }

            await _articleContext.DeleteOld();
        }
    }
}