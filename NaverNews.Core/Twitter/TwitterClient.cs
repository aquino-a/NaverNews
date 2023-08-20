using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace NaverNews.Core
{
    public class TwitterClient
    {
        private const string BASE_URL = "https://api.twitter.com/";
        private readonly AuthenticationHeaderValue _basicAuth;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;

        private DateTime _expireTime;
        private Tokens _tokens;

        public TwitterClient(string clientId, string clientSecret, HttpClient httpClient)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _basicAuth = CreateBasicHeader(clientId, clientSecret);
            _httpClient = httpClient;
        }

        public Tokens Tokens
        {
            get => _tokens;
            set
            {
                _tokens = value;

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Tokens.Access);
            }
        }

        public async Task<string> Post(string postContent)
        {
            var json = new JsonObject();
            json.Add("text", JsonValue.Create<string>(postContent));

            var content = JsonContent.Create(json);

            var response = await _httpClient.PostAsync(BASE_URL + "2/tweets", content);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync();
            var root = JsonNode.Parse(raw);

            return root["data"]["id"].ToString();
        }

        public async Task Refresh()
        {
            if (await IsValid())
            {
                return;
            }

            var pairs = new Dictionary<string, string>()
            {
                { "refresh_token", Tokens.Refresh },
                { "grant_type", "refresh_token" }
            };

            var content = new FormUrlEncodedContent(pairs);

            _httpClient.DefaultRequestHeaders.Authorization = _basicAuth;
            var response = await _httpClient.PostAsync(BASE_URL + "2/oauth2/token", content);
            var json = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var root = JsonNode.Parse(json);

            Tokens = new Tokens
            {
                Refresh = root["refresh_token"].ToString(),
                Access = root["access_token"].ToString()
            };

            _expireTime = DateTime.Now.AddHours(1.9);
        }

        private AuthenticationHeaderValue CreateBasicHeader(string clientId, string clientSecret)
        {
            var encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            return new AuthenticationHeaderValue("Basic", encodedString);
        }

        private async Task<bool> IsAuthorized()
        {
            var response = await _httpClient.GetAsync(BASE_URL + "2/users/me");

            return response.IsSuccessStatusCode;
        }

        private async Task<bool> IsValid()
        {
            if (_expireTime == default(DateTime))
            {
                return await IsAuthorized();
            }

            return DateTime.Now < _expireTime;
        }
    }
}