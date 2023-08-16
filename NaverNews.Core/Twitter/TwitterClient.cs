using System.Net.Http.Json;
using System.Text.Json.Nodes;
using static System.Net.Mime.MediaTypeNames;

namespace NaverNews.Core
{
    public class TwitterClient
    {
        private const string BASE_URL = "https://api.twitter.com/";
        private const string REFRESH_PATH = "./refresh.token";
        private readonly string _clientId;
        private readonly HttpClient _httpClient;

        public TwitterClient(string clientId, HttpClient httpClient)
        {
            _clientId = clientId;
            _httpClient = httpClient;
        }

        internal Tokens Tokens { get; set; }

        public async Task LoadRefreshToken()
        {
            if (!File.Exists(REFRESH_PATH))
            {
                return;
            }

            var refresh = await File.ReadAllTextAsync(REFRESH_PATH);

            Tokens = new Tokens
            {
                Refresh = refresh
            };
        }

        public async Task Refresh()
        {
            var pairs = new Dictionary<string, string>()
            {
                { "refresh_token", Tokens.Refresh },
                { "grant_type", "refresh_token" },
                { "client_id", _clientId },
            };

            var content = new FormUrlEncodedContent(pairs);

            var response = await _httpClient.PostAsync(BASE_URL + "2/oauth2/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var root = JsonNode.Parse(json);

            Tokens = new Tokens
            {
                Refresh = root["refresh_token"].ToString(),
                Access = root["access_token"].ToString()
            };

            await File.WriteAllTextAsync(REFRESH_PATH, Tokens.Refresh);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Tokens.Access}");
        }

        //scopes required
        //tweet.read
        //tweet.write
        //users.read
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
    }
}