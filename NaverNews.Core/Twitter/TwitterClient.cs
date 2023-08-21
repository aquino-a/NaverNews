using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace NaverNews.Core
{
    public class TwitterClient
    {
        internal const string BASE_URL = "https://api.twitter.com/";
        private readonly HttpClient _httpClient;
        private readonly ILogger<TwitterClient> _logger;
        private readonly TokenService _tokenService;

        public TwitterClient(TokenService tokenService,
                             HttpClient httpClient,
                             ILogger<TwitterClient> logger)
        {
            _tokenService = tokenService;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> Post(string postContent)
        {
            var json = new JsonObject();
            json.Add("text", JsonValue.Create<string>(postContent));

            var content = JsonContent.Create(json);

            var response = await _httpClient.PostAsync(BASE_URL + "2/tweets", content);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Post was forbidden: {responseContent}");

                return responseContent;
            }

            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync();
            var root = JsonNode.Parse(raw);

            return root["data"]["id"].ToString();
        }

        public async Task Refresh()
        {
            var accessToken = await _tokenService.GetAccessToken();

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}