namespace NaverNews.Core
{
    public class ArticleService
    {
        private readonly ArticleDbContext _articleContext;
        private readonly IChatGptService _chatGptService;
        private readonly NaverClient _client;
        private readonly TwitterClient _twitterClient;

        public ArticleService(NaverClient client,
                              IChatGptService chatGptService,
                              TwitterClient twitterClient,
                              ArticleDbContext articleContext)
        {
            _client = client;
            _chatGptService = chatGptService;
            _twitterClient = twitterClient;
            _articleContext = articleContext;
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

            if (!string.IsNullOrWhiteSpace(article.Summary))
            {
                return article.Summary;
            }

            var summary = await _chatGptService.Summarize(article.Text);

            article.Summary = summary;
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

            var id = await _twitterClient.Post(article.Summary);

            article.TwitterId = id;
            await _articleContext.SaveChangesAsync();

            return id;
        }

        public async Task<int> SearchArticles(NewsType type, int pages)
        {
            var searchResult = new SearchResult { StartTime = DateTime.UtcNow };

            var articles = await _client.GetArticles(type, pages);

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