using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace NaverNews.Core
{
    public class ChatGptService : IChatGptService
    {
        private const string COMPLETION_URL = "https://api.openai.com/v1/chat/completions";
        private const string SUMMARIZE_SYSTEM = "use headlinese to concisely shorten the article to less than 35 words with short English sentences.";

        private readonly HttpClient _httpClient;
        private readonly string _modelName;

        public ChatGptService(HttpClient httpClient, string apiKey, string modelName = "gpt-3.5-turbo")
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _modelName = modelName;
        }

        public async Task<string> Summarize(string text)
        {
            var body = GetDefault(SUMMARIZE_SYSTEM, text);
            var response = await _httpClient.PostAsync(COMPLETION_URL, JsonContent.Create(body));
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var root = JsonNode.Parse(responseContent);

            return root["choices"][0]["message"]["content"].ToString();
        }

        public async Task<string> Translate(string text)
        {
            throw new NotSupportedException("not needed for now.");
        }

        private Body GetDefault(string systemContent, string userContent)
        {
            return new Body
            {
                FrequencyPenalty = 0,
                MaxTokens = 500,
                Model = _modelName,
                PresencePenalty = 0,
                Temperature = 0,
                TopP = 0,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "system",
                        Content = systemContent
                    },
                    new Message
                    {
                        Role = "user",
                        Content = userContent
                    },
                },
            };
        }
    }
}