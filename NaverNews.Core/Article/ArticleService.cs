namespace NaverNews.Core
{
    public class ArticleService
    {
        private readonly ArticleDbContext _articleContext;
        private readonly IChatGptService _chatGptService;
        private readonly NaverClient _client;

        public ArticleService(NaverClient client,
                              IChatGptService chatGptService,
                              ArticleDbContext articleContext)
        {
            _client = client;
            _chatGptService = chatGptService;
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
                .Where(a => a.Time >= olderThan)
                .Where(a => a.Total >= minimumTotal)
                .Take(count);
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