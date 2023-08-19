using Microsoft.Extensions.Logging;

namespace NaverNews.Core
{
    public class ArticleService : IArticleService
    {
        private readonly ArticleDbContext _articleContext;
        private readonly ILogger<ArticleService> _logger;
        private readonly IChatGptService _chatGptService;
        private readonly NaverClient _client;
        private readonly TwitterClient _twitterClient;

        public ArticleService(NaverClient client,
                              IChatGptService chatGptService,
                              TwitterClient twitterClient,
                              ArticleDbContext articleContext,
                              ILogger<ArticleService> logger)
        {
            _client = client;
            _chatGptService = chatGptService;
            _twitterClient = twitterClient;
            _articleContext = articleContext;
            _logger = logger;
        }

        public int SkipThreshhold { get; set; } = 50;
        public int SearchPageCount { get; set; } = 20;
        public int EngagementMinimum { get; set; } = 1000;

        public async Task AutoPost()
        {
            _logger.LogInformation($"Starting auto post for {NewsType.Society}");
            var count = await SearchArticles(NewsType.Society, SearchPageCount);
            _logger.LogInformation($"Found {count} new articles.");

            var lastTime = GetLastAutoPostTime();

            var articles = _articleContext.Articles
                .Where(a => a.Time >= lastTime)
                .Where(a => a.ReplyCount + a.CommentCount >= EngagementMinimum)
                .ToList();

            if (articles == null || articles.Count == 0)
            {
                _logger.LogInformation($"No article since last post met engagement minimum. [{EngagementMinimum}]");
                return;
            }

            _logger.LogInformation($"Found {articles.Count} articles that meet the minimum. [{EngagementMinimum}]");

            await _twitterClient.Refresh();
            foreach (var article in articles)
            {
                await AutoPost(article);
            }
        }

        public async Task<string> GetArticleText(string articleId)
        {
            var article = await _articleContext.Articles.FindAsync(articleId);
            if (article == null)
            {
                throw new ArticleNotFoundException();
            }

            return await GetArticleText(article);
        }

        public IEnumerable<Article> GetByTimeAndTotal(DateTime olderThan, int minimumTotal = 10, int count = 20)
        {
            return _articleContext.Articles
                .OrderByDescending(a => a.Time)
                .Where(a => a.Time <= olderThan)
                .Where(a => a.Total >= minimumTotal)
                .Take(count);
        }

        public async Task<IEnumerable<SearchResult>> GetSearchResults(DateTime olderThanUtc, int count)
        {
            return _articleContext.SearchResults
                .OrderByDescending(r => r.EndTime)
                .Where(r => r.EndTime <= olderThanUtc)
                .Take(count);
        }

        public async Task<string> GetSummary(string articleId)
        {
            var article = await _articleContext.Articles.FindAsync(articleId);
            if (article == null)
            {
                throw new ArticleNotFoundException();
            }

            return await GetSummary(article);
        }

        public async Task<string> Post(string articleId)
        {
            var article = await _articleContext.Articles.FindAsync(articleId);
            if (article == null)
            {
                throw new ArticleNotFoundException();
            }

            if (!string.IsNullOrWhiteSpace(article.Summary))
            {
                throw new ArticleSummaryNotReadyException();
            }

            await _twitterClient.Refresh();
            var id = await _twitterClient.Post(article.Summary);

            article.TwitterId = id;
            article.IsOnTwitter = true;
            await _articleContext.SaveChangesAsync();

            return id;
        }

        public async Task<int> SearchArticles(NewsType type, int pages)
        {
            var searchResult = new SearchResult { StartTime = DateTime.UtcNow };

            var articles = await _client.GetArticles(type, pages);
            articles = articles.Where(a => a.Total >= SkipThreshhold).ToList();

            articles.ForEach(a =>
            {
                var existingArticle = _articleContext.Find<Article>(a.ArticleId);
                if (existingArticle == null)
                {
                    _articleContext.Add(a);
                }
                else
                {
                    existingArticle.CommentCount = a.CommentCount;
                    existingArticle.ReplyCount = a.ReplyCount;
                }
            });

            var changeCount = await _articleContext.SaveChangesAsync();
            searchResult.Count = changeCount;
            searchResult.EndTime = DateTime.UtcNow;

            _articleContext.SearchResults.Add(searchResult);
            await _articleContext.SaveChangesAsync();

            return changeCount;
        }

        private async Task AutoPost(Article article)
        {
            await GetArticleText(article);
            await GetSummary(article);
            var id = await _twitterClient.Post(article.Summary);

            article.WasAutoPosted = true;
            article.TwitterId = id;
            article.IsOnTwitter = true;
            await _articleContext.SaveChangesAsync();

            _logger.LogInformation($"Posted article ({article.ArticleUrl}) to twitter. ({article.TwitterId}");
        }

        private async Task<string> GetArticleText(Article article)
        {
            if (!string.IsNullOrWhiteSpace(article.Text))
            {
                return article.Text;
            }

            var text = await _client.GetArticleText(article);

            article.Text = text;
            await _articleContext.SaveChangesAsync();

            return text;
        }

        private DateTime GetLastAutoPostTime()
        {
            var lastPost = _articleContext.Articles
                .Where(a => a.WasAutoPosted)
                .OrderByDescending(a => a.Time)
                .FirstOrDefault();

            _logger.LogInformation($"Last auto-posted article was {lastPost?.ArticleId}. ({lastPost?.ArticleUrl})");

            var lastTime = lastPost == null
                ? default(DateTime)
                : lastPost.Time;

            return lastTime;
        }

        private async Task<string> GetSummary(Article article)
        {
            if (string.IsNullOrWhiteSpace(article.Text))
            {
                throw new ArticleTextNotReadyException();
            }

            if (!string.IsNullOrWhiteSpace(article.TranslatedSummary))
            {
                return article.TranslatedSummary;
            }

            var summary = await _chatGptService.Summarize(article.Text);

            article.TranslatedSummary = summary;
            await _articleContext.SaveChangesAsync();

            return summary;
        }
    }
}