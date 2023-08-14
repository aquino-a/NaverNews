using System.Text.Json.Serialization;

namespace NaverNews.Core
{
    internal class Body
    {
        [JsonPropertyName("frequency_penalty")]
        public int FrequencyPenalty { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("presence_penalty")]
        public int PresencePenalty { get; set; }

        [JsonPropertyName("temperature")]
        public int Temperature { get; set; }

        [JsonPropertyName("top_p")]
        public int TopP { get; set; }
    }
}