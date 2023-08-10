namespace NaverNews.Core.Tests
{
    public class ClientTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GetArticlesTest()
        {
            var httpClient = new HttpClient();
            var nc = new NaverClient(httpClient);

            var articles = await nc.GetArticles(NewsType.Society, 1);

            Assert.That(articles, Is.Not.Null);
            Assert.That(articles.Count, Is.GreaterThanOrEqualTo(20));

            var firstArticle = articles[0];
            Assert.That(firstArticle.ArticleId, Is.Not.Empty);
            Assert.That(firstArticle.OfficeId, Is.Not.Empty);
            Assert.That(firstArticle.Title, Is.Not.Empty);
            Assert.That(firstArticle.Summary, Is.Not.Empty);
            Assert.That(firstArticle.Time, Is.Not.EqualTo(default(DateTime)));
            Assert.That(firstArticle.ImageUrl, Is.Not.Empty);
            Assert.That(firstArticle.ArticleUrl, Is.Not.Empty);
        }
    }
}