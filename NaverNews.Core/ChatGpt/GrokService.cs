using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NaverNews.Core
{
    public class GrokService : IChatGptService
    {
//         curl https://api.x.ai/v1/chat/completions \
//   -H "Content-Type: application/json" \
//   -H "Authorization: Bearer $XAI_API_KEY" \
//   -d '{
//         "messages": [
//           {
//             "role": "system",
//             "content": "You are Grok, a chatbot inspired by the Hitchhikers Guide to the Galaxy."
//           },
//           {
//             "role": "user",
//             "content": "What is the meaning of life, the universe, and everything?"
//           }
//         ],
//         "model": "grok-beta",
//         "stream": false,
//         "temperature": 0
//       }'
        private const string COMPLETION_URL = "https://api.x.ai/v1/chat/completions";
        private const string SUMMARIZE_SYSTEM = "tweet,summarize in english,headlinese,no header,280 character limit:";

        private readonly HttpClient _httpClient;

        public GrokService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
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
                Model = "gpt-3.5-turbo",
                Temperature = 0,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "system",
                        Content = systemContent,
                    },
                    new Message
                    {
                        Role = "user",
                        Content = userContent,
                    },
                },
            };
        }
        
//   -d '{
//         "messages": [
//           {
//             "role": "system",
//             "content": "You are Grok, a chatbot inspired by the Hitchhikers Guide to the Galaxy."
//           },
//           {
//             "role": "user",
//             "content": "What is the meaning of life, the universe, and everything?"
//           }
//         ],
//         "model": "grok-beta",
//         "stream": false,
//         "temperature": 0
//       }'
        private class Body
        {
            [JsonPropertyName("messages")]
            public List<Message> Messages { get; set; }

            [JsonPropertyName("model")]
            public string Model { get; set; }

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }

            [JsonPropertyName("temperature")]
            public int Temperature { get; set; }
        }
    }
}