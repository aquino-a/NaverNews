namespace NaverNews.Core.Tests
{
    public class ClientTests
    {
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

        [Test]
        public async Task GetArticleTextTest()
        {
            var article = new Article
            {
                ArticleUrl = "https://n.news.naver.com/mnews/article/023/0003781854?sid=102"
            };

            var nc = new NaverClient(new HttpClient());

            var text = await nc.GetArticleText(article);

            Assert.That(text, Is.Not.Null);
            Assert.That(text.Length, Is.GreaterThan(1));
        }

        [SetUp]
        public void Setup()
        {
        }
    }
}