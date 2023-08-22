using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace NaverNews.Core.Tests
{
    public class TwitterTests
    {
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddUserSecrets("30334a78-31c6-4674-a662-4d7f4620eec8")
                .Build();
        }

        [Test]
        public async Task TokenRefreshTest()
        {
            var connectionString = _configuration["Cosmos-Connection"];
            Assert.That(connectionString, Is.Not.Null);

            var options = new DbContextOptionsBuilder<TokenDbContext>()
                .UseCosmos(connectionString, "news").Options;

            using (var tokenContext = new TokenDbContext(options))
            {
                var httpClient = new HttpClient();
                var logger = new Mock<ILogger<TokenService>>();
                var tc = new TokenService(_configuration["Twitter:clientId"]!,
                                           _configuration["Twitter:clientSecret"]!,
                                           tokenContext,
                                           httpClient,
                                           logger.Object);
                await tc.Load();
                await tc.Refresh();
                Assert.Pass();
            }
        }
    }
}