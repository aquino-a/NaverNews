using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace NaverNews.Core.Tests
{
    public class DbContextTests
    {
        private IConfiguration _configuration;

        [SetUp]
        public void Init()
        {
            //_configuration = TestContext.CurrentContext.TestDirectory);
            _configuration = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddUserSecrets("30334a78-31c6-4674-a662-4d7f4620eec8")
                .Build();
        }

        [Test]
        public async Task DeleteTest()
        {
            var connectionString = _configuration["Cosmos-Connection"];
            Assert.That(connectionString, Is.Not.Null);

            var options = new DbContextOptionsBuilder<ArticleDbContext>()
                .UseCosmos(connectionString, "news").Options;

            using (var articleContext = new ArticleDbContext(options))
            {
                await articleContext.DeleteOld();
                Assert.Pass();
            }
        }
    }
}