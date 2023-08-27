using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace NaverNews.Core
{
    public class ArticleService : IArticleService
    {
        private readonly ArticleDbContext _articleContext;
        private readonly IChatGptService _chatGptService;
        private readonly NaverClient _client;
        private readonly ILogger<ArticleService> _logger;
        private readonly TwitterClient _twitterClient;
        private readonly Regex KOREAN_REGEX = new Regex("[\uAC00-\uD7AF]{4,}", RegexOptions.Compiled);

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

        public int EngagementMinimum { get; set; } = 1000;
        public int SearchPageCount { get; set; } = 20;
        public int SkipThreshhold { get; set; } = 50;
        public int TrimLength { get; set; } = 280;
        public int UrlLength { get; set; } = 24;

        public async Task AutoPost()
        {
            _logger.LogInformation($"Starting auto post for {NewsType.Society}");
            var articles = await SearchArticles(NewsType.Society, SearchPageCount);
            _logger.LogInformation($"Found {articles.Count} new articles.");

            articles = articles
                .Where(a => string.IsNullOrWhiteSpace(a.TwitterId))
                .Where(a => a.ReplyCount + a.CommentCount >= EngagementMinimum)
                .OrderByDescending(a => a.Time)
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

        public async Task AutoPost(string articleId)
        {
            var existingArticle = _articleContext.Find<Article>(articleId);

            if (existingArticle == null)
            {
                throw new ArticleNotFoundException();
            }

            // used only from controller where one is posted.
            await _twitterClient.Refresh();
            await AutoPost(existingArticle);
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
                .Where(a => a.ReplyCount + a.CommentCount >= minimumTotal)
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

            if (!string.IsNullOrWhiteSpace(article.TranslatedSummary))
            {
                throw new ArticleSummaryNotReadyException();
            }

            await _twitterClient.Refresh();
            var id = await _twitterClient.Post(article.TranslatedSummary);

            article.TwitterId = id;
            article.IsOnTwitter = true;
            await _articleContext.SaveChangesAsync();

            return id;
        }

        public async Task<List<Article>> SearchArticles(NewsType type, int pages)
        {
            var searchResult = new SearchResult { StartTime = DateTime.UtcNow };
            _articleContext.SearchResults.Add(searchResult);
            await _articleContext.SaveChangesAsync();

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

            await _articleContext.SaveChangesAsync();

            return articles;
        }

        private string AddUrl(string translatedSummary, string articleUrl)
        {
            if (translatedSummary.Length <= TrimLength - UrlLength)
            {
                return $"{translatedSummary} {articleUrl}";
            }

            return translatedSummary;
        }

        private async Task AutoPost(Article article)
        {
            await GetArticleText(article);
            await GetSummary(article);

            var translatedSummary = article.TranslatedSummary;
            if (translatedSummary.Length >= TrimLength)
            {
                _logger.LogError($"Translated summary for {article.ArticleId} is too large. [{translatedSummary.Length}]");
                translatedSummary = Trim(translatedSummary);
                if (translatedSummary.Length <= 0)
                {
                    _logger.LogError($"Translated summary couldn't be trimmed [{translatedSummary.Length}]");
                    article.WasAutoPosted = true;
                    article.TwitterId = "trim fail";
                    await _articleContext.SaveChangesAsync();

                    return;
                }
            }

            if (KOREAN_REGEX.IsMatch(translatedSummary))
            {
                _logger.LogError($"Translated summary had too much Korean. Skipped.");
                return;
            }

            translatedSummary = AddUrl(translatedSummary, article.ArticleUrl);

            var id = await _twitterClient.Post(translatedSummary);

            article.WasAutoPosted = true;
            article.TwitterId = id;
            article.IsOnTwitter = true;
            await _articleContext.SaveChangesAsync();

            _logger.LogInformation($"Posted article ({article.ArticleUrl}) to twitter. ({article.TwitterId})");
        }

        private async Task<string> GetArticleText(Article article)
        {
            if (!string.IsNullOrWhiteSpace(article.Text))
            {
                return article.Text;
            }

            var text = await _client.GetArticleText(article);

            article.Text = HttpUtility.HtmlDecode(text);
            await _articleContext.SaveChangesAsync();

            return text;
        }

        private DateTime GetLastAutoPostTime()
        {
            var lastPost = _articleContext.Articles
                .Where(a => a.WasAutoPosted)
                .OrderByDescending(a => a.Time)
                .FirstOrDefault();

            _logger.LogWarning($"Last auto-posted article was {lastPost?.ArticleId}. ({lastPost?.ArticleUrl})");

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

        private string Trim(string translatedSummary)
        {
            var shortenedResult = new StringBuilder(translatedSummary);
            for (
                int i = translatedSummary.LastIndexOf('.'), tempLength = shortenedResult.Length;
                shortenedResult.Length >= TrimLength;
                i = translatedSummary.LastIndexOf('.', i - 1), tempLength = shortenedResult.Length)
            {
                if (i == -1)
                {
                    return string.Empty;
                }

                shortenedResult.Remove(i, shortenedResult.Length - i);
                _logger.LogInformation($"Trimmed off {tempLength - shortenedResult.Length}");
            }

            return shortenedResult.ToString();
        }
    }
}