using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaverNews.Core.Twitter;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace NaverNews.Core
{
    public class TokenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TokenService> _logger;
        private readonly object _refreshLock = new object();
        private readonly TokenContext _tokenContext;

        public TokenService(string clientId,
                            string clientSecret,
                            TokenContext tokenContext,
                            HttpClient httpClient,
                            ILogger<TokenService> logger)
        {
            _tokenContext = tokenContext;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = CreateBasicHeader(clientId, clientSecret);
            _logger = logger;
        }

        public Tokens Tokens { get; set; }

        public async Task<string> GetAccessToken()
        {
            await Task.Factory.StartNew(() =>
            {
                lock (_refreshLock)
                {
                    Refresh().GetAwaiter().GetResult();
                }
            });

            return Tokens.Access;
        }

        public async Task Load()
        {
            Tokens = await _tokenContext.Tokens.FirstAsync();
        }

        public async Task Refresh()
        {
            if (await IsValid())
            {
                _logger.LogInformation("Token is still valid");
                return;
            }

            _logger.LogInformation("Token is not valid. Starting refresh.");

            var content = GetRefreshContent();
            var response = await _httpClient.PostAsync(TwitterClient.BASE_URL + "2/oauth2/token", content);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Token refresh fail: {Json}", json);
            }

            response.EnsureSuccessStatusCode();

            var root = JsonNode.Parse(json);

            var tokens = await _tokenContext.Tokens.FirstAsync();

            tokens.Refresh = root["refresh_token"].ToString();
            tokens.Access = root["access_token"].ToString();
            tokens.ExpireTime = DateTime.UtcNow.AddHours(2);

            await _tokenContext.SaveChangesAsync();
        }

        private AuthenticationHeaderValue CreateBasicHeader(string clientId, string clientSecret)
        {
            var encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            return new AuthenticationHeaderValue("Basic", encodedString);
        }

        private FormUrlEncodedContent GetRefreshContent()
        {
            var pairs = new Dictionary<string, string>()
            {
                { "refresh_token", Tokens.Refresh },
                { "grant_type", "refresh_token" },
            };

            return new FormUrlEncodedContent(pairs);
        }

        private async Task<bool> IsAuthorized()
        {
            var response = await _httpClient.GetAsync(TwitterClient.BASE_URL + "2/users/me");

            return response.IsSuccessStatusCode;
        }

        private async Task<bool> IsValid()
        {
            if (Tokens.ExpireTime == default(DateTime))
            {
                return await IsAuthorized();
            }

            return DateTime.UtcNow < Tokens.ExpireTime;
        }
    }
}