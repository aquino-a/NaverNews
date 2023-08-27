namespace NaverNews.Core
{
    public interface IArticleService
    {
        Task AutoPost();

        Task AutoPost(string articleId);

        Task<string> GetArticleText(string articleId);

        IEnumerable<Article> GetByTimeAndTotal(DateTime olderThan, int minimumTotal = 10, int count = 20);

        Task<IEnumerable<SearchResult>> GetSearchResults(DateTime olderThanUtc, int count);

        Task<string> GetSummary(string articleId);

        Task<string> Post(string articleId);

        Task<List<Article>> SearchArticles(NewsType type, int pages);
    }
}