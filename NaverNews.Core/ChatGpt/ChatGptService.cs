using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace NaverNews.Core
{
    public class ChatGptService : IChatGptService
    {
        //{
        //  "model": "gpt-3.5-turbo-16k",
        //  "messages": [
        //    {
        //      "role": "system",
        //      "content": "in less than 35 words, concisely summarize the articles :"
        //    },
        //    {
        //      "role": "user",
        //      "content": "롤스로이스 차량을 몰아 인도로 돌진해 20대 여성을 다치게 한 혐의로 경찰 수사를 받았던 남성에 대해 경찰이 보강 수사 후 구속영장을 신청하겠다는 계획을 밝혔다.\n\n서울경찰청 관계자는 14일 기자간담회에서 \"(롤스로이스 운전자 신모씨를) 현행범 체포 이후 구속영장을 신청하려 했는데, 수사 과정에서 의사가 '신 씨가 사고 3일 전 케타민을 투약했다'는 취지의 전화를 조사관에게 했다\"고 말했다.\n\n그러면서 \"3일이면 약물이 빠져나가기 충분한 시간이어서 약물 운전에 따른 위험 운전으로 영장을 신청하기 부족하다고 판단해 석방했다\"고 설명했다.\n\n신씨 체포 당시 구속 사유가 충분하지 않아 영장이 받아들여지지 않을 가능성이 컸던 만큼 추가 행적 조사 및 보강 수사 후 구속 영장을 신청할 계획이었다고 해명했다.\n\n20대 운전자인 신씨는 지난 2일 오후 8시 10분쯤 서울시 강남구 압구정역 인근에서 롤스로이스를 몰다 인도로 돌진했다. 이 사고로 20대 여성이 다쳤다. 피해 여성은 머리와 다리 등을 크게 다쳐 수술받았으나 뇌사 상태로 알려졌다.\n\n당시 신씨는 병원에서 향정신성의약품을 처방받아 투약 후 운전대를 잡은 것으로 조사됐다.\n\n신씨는 경찰 조사를 받고 17시간 만에 석방돼 논란이 됐다. 이후 경찰은 지난 9일 신씨에게 특정범죄가중처벌법상 위험운전치상과 도로교통법상 약물운전 혐의를 적용해 구속영장을 신청했고, 서울중앙지법은 구속영장을 발부했다.\n\n신씨의 법률대변인이 대형 로펌 소속 전관 변호사라는 점에서 석방에 영향을 미친 것이 아니냐는 의혹이 제기됐지만, 경찰은 \"(변호사가) 신원보증하겠다고 말은 했지만, 변호사보다 사건의 수사 완결성을 기하기 위해 (석방)했던 것\"이라고 답했다.\n\n또 \"국립과학수사연구원 약물 성분 검사를 의뢰했는데 향정신성의약품 복용만 나오고 마약류는 (복용한 결과가) 없다\"며 \"마약수사대에서 (신씨가 복용한 약물 및 처방의 적절성에 대해) 별도로 조사 중\"이라면서 교통사고와 별도로 마약 사건을 수사 중이라고 밝혔다.\n"
        //    },
        //    {
        //    "role": "assistant",
        //      "content": "The police in Seoul plan to apply for an arrest warrant for the man who drove a Rolls-Royce into a pedestrian, injuring her, after conducting additional investigations. The man was previously released due to insufficient evidence."
        //    }
        //  ],
        //  "temperature": 0,
        //  "max_tokens": 500,
        //  "top_p": 0,
        //  "frequency_penalty": 0,
        //  "presence_penalty": 0
        //}
        private const string COMPLETION_URL = "https://api.openai.com/v1/chat/completions";
        private const string SUMMARIZE_SYSTEM = "in less than 35 words, concisely summarize the articles :";

        private readonly HttpClient _httpClient;

        public ChatGptService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authentication", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }

        public async Task<string> Summarize(string text)
        {
            var body = GetDefault(SUMMARIZE_SYSTEM, text);
            var response = await _httpClient.PostAsync(COMPLETION_URL, JsonContent.Create(body));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var root = JsonNode.Parse(content);

            return root["choices"][0]["content"].ToString();
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
                Model = "gpt-3.5-turbo-16k",
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