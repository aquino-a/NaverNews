namespace NaverNews.Core
{
    public class ArticleService
    {
        private readonly ArticleDbContext _articleContext;
        private readonly NaverClient _client;

        private Task<List<Article>> _currentTask;

        public ArticleService(NaverClient client, ArticleDbContext articleContext)
        {
            _client = client;
            _articleContext = articleContext;
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
            try
            {
                if (_currentTask != null)
                {
                    return -1;
                }

                var searchResult = new SearchResult { StartTime = DateTime.UtcNow };

                _currentTask = _client.GetArticles(type, pages);
                var articles = await _currentTask;

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
            finally
            {
                _currentTask = null;
            }
        }
    }
}