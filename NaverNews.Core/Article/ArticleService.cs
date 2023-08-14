namespace NaverNews.Core
{
    public class ArticleService
    {
        private readonly ArticleDbContext _articleContext;
        private readonly NaverClient _client;

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