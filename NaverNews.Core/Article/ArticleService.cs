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
        public int EngagementMinimum { get; set; } = 200;

        public async Task AutoPost()
        {
            var count = await SearchArticles(NewsType.Society, SearchPageCount);

            var article = _articleContext.Articles
                .OrderByDescending(a => a.Time)
                .TakeWhile(a => !a.WasAutoPosted)
                .Where(a => a.Total >= EngagementMinimum)
                .MaxBy(a => a.Total);

            if (article == null)
            {
                _logger.LogInformation($"No article since last post met engagement minimum. [{EngagementMinimum}]");
                return;
            }

            await _twitterClient.Refresh();
            var id = await _twitterClient.Post(article.Summary);

            article.WasAutoPosted = true;
            article.TwitterId = id;
            article.IsOnTwitter = true;
            await _articleContext.SaveChangesAsync();
        }

        public async Task<string> GetArticleText(string articleId)
        {
            var article = await _articleContext.Articles.FindAsync(articleId);
            if (article == null)
            {
                throw new ArticleNotFoundException();
            }

            if (!string.IsNullOrWhiteSpace(article.Text))
            {
                return article.Text;
            }

            var text = await _client.GetArticleText(article);

            article.Text = text;
            await _articleContext.SaveChangesAsync();

            return text;
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
                var existingArticle = _articleContext.Find<Article>(a);
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
    }
}