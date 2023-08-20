using Microsoft.Extensions.Configuration;

namespace NaverNews.Core.Tests
{
    public class TwitterTests
    {

        private IConfiguration _configuration;

        [Test]
        public async Task RefreshTest()
        {
            var httpClient = new HttpClient();
            var tc = new TwitterClient(_configuration["Twitter:clientId"]!, _configuration["Twitter:clientSecret"]!, httpClient);
            tc.Tokens = new Tokens
            {
                Access = _configuration["Twitter:accessToken"]!,
                Refresh = _configuration["Twitter:refreshToken"]!
            };

            await tc.Refresh();
            Assert.Pass();
        }

        [SetUp]
        public void Setup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddUserSecrets("30334a78-31c6-4674-a662-4d7f4620eec8")
                .Build();
        }
    }
}