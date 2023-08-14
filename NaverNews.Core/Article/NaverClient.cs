using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace NaverNews.Core
{
    public class NaverClient
    {
        private const string ARTICLE_URL = "https://n.news.naver.com/mnews/article";
        private const string BASE_URL = "https://news.naver.com/";
        private const string COMMENT_PATH = "api/comment/listCount.json";
        private const string LIST_PATH = "main/mainNews.naver";
        private const string SECTION_PATH = "main/main.naver";
        private readonly HttpClient _httpClient;

        static NaverClient()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public NaverClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
            _httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            _httpClient.DefaultRequestHeaders.Add("dnt", "1");
            _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://news.naver.com/main/main.naver?mode=LSD&mid=shm&sid1=102");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            _httpClient.DefaultRequestHeaders.Add("Sec-Gpc", "1");
            _httpClient.DefaultRequestHeaders.Add("Origin", "empty");
        }

        /// <summary>
        /// Gets basic article data from naver.
        /// </summary>
        /// <param name="type">the type of news section to get.</param>
        /// <param name="pages">amount of pages deep to check</param>
        /// <returns></returns>
        public async Task<List<Article>> GetArticles(NewsType type, int pages)
        {
            var mainPageResponse = await _httpClient.GetAsync(BASE_URL + SECTION_PATH);
            mainPageResponse.EnsureSuccessStatusCode();

            //mode=LSD&mid=shm&sid1=102
            var sectionResponse = await _httpClient.GetAsync($"{BASE_URL}{SECTION_PATH}?mode=LSD&mid=shm&sid1={(int)type}");
            sectionResponse.EnsureSuccessStatusCode();

            var articles = new List<Article>(pages * 20);

            //first list //sid1=102&firstLoad=Y
            articles.AddRange(await GetArticles(type, $"sid1={(int)type}&firstLoad=Y"));
            for (int i = 2; i <= pages; i++)
            {
                // next pages //sid1=102&date=%2000:00:00&page=2
                articles.AddRange(await GetArticles(type, $"sid1={(int)type}&date=%2000:00:00&page={i}"));
            }

            return articles;
        }

        private void AddComments(List<Article> articles, string rawComments)
        {
            //{
            //    "success": true,
            //    "code": "1000",
            //    "message": "요청을 성공적으로 처리하였습니다.",
            //    "lang": "ko",
            //    "country": "KR",
            //    "result": {
            //        "news057,0001761655": {
            //            "comment": 0,
            //            "reply": 0,
            //            "delCommentByUser": null,
            //            "delCommentByMon": null,
            //            "blindCommentByUser": null,
            //            "blindReplyByUser": null,
            //            "exposureConfig": {
            //                "reason": null,
            //                "status": "COMMENT_ON"
            //            },
            //            "count": 0
            //        },
            //        "news016,0002181766": {
            //            "comment": 10,
            //            "reply": 0,
            //            "delCommentByUser": null,
            //            "delCommentByMon": null,
            //            "blindCommentByUser": null,
            //            "blindReplyByUser": null,
            //            "exposureConfig": {
            //                "reason": null,
            //                "status": "COMMENT_ON"
            //            },
            //            "count": 10
            //        },
            //        "news023,0003780612": {
            //            "comment": 8,
            //            "reply": 3,
            //            "delCommentByUser": null,
            //            "delCommentByMon": null,
            //            "blindCommentByUser": null,
            //            "blindReplyByUser": null,
            //            "exposureConfig": {
            //                "reason": null,
            //                "status": "COMMENT_ON"
            //            },
            //            "count": 11
            //        }
            //    },
            //    "date": "2023-08-10T00:56:57+0000"
            //}

            var root = JsonNode.Parse(rawComments);
            var resultNode = root["result"]; //regular node (not array)
            articles.ForEach(a =>
            {
                var match = resultNode[a.ObjectName];
                if (match == null)
                {
                    return;
                }

                a.ReplyCount = int.Parse(match["reply"].ToString());
                a.CommentCount = int.Parse(match["comment"].ToString());
            });
        }

        private string CreateObjectParams(IEnumerable<Article> articles)
        {
            var queryParams = articles
                .Select(a => $"objectId={a.ObjectName}");

            return string.Join('&', queryParams);
        }

        private string DecodeEucKr(byte[] rawBytes)
        {
            return Encoding.GetEncoding(51949).GetString(rawBytes);
        }

        private async Task<IEnumerable<Article>> GetArticles(NewsType type, string queryParams)
        {
            var listResponse = await _httpClient.GetAsync($"{BASE_URL}{LIST_PATH}?{queryParams}");
            listResponse.EnsureSuccessStatusCode();

            var rawBytes = await listResponse.Content.ReadAsByteArrayAsync();
            var raw = DecodeEucKr(rawBytes);
            var articles = ParseArticles(raw, type).ToList();

            var objectParams = CreateObjectParams(articles);

            // comment counts //resultType=MAP&ticket=news&lang=ko&country=KR&objectId=news001,0014121080&objectId=news001,0014122715&objectId=news055,0001080206&objectId=news022,0003843225&objectId=news009,0005170711&objectId=news057,0001761430&objectId=news421,0006982201&objectId=news005,0001629825&objectId=news003,0012022543&objectId=news023,0003780680&objectId=news214,0001291803&objectId=news214,0001291802&objectId=news214,0001291537&objectId=news277,0005298389&objectId=news014,0005055168&objectId=news277,0005298385&objectId=news008,0004923507&objectId=news055,0001080338&objectId=news421,0006982405&objectId=news016,0002181672&objectId=news422,0000613595&objectId=news052,0001921379&objectId=news016,0002181670&objectId=news006,0000119269&objectId=news009,0005170770&objectId=news056,0011543018&objectId=news011,0004224778&objectId=news586,0000062604&objectId=news028,0002651718&objectId=news003,0012023609&objectId=news025,0003299815&objectId=news469,0000754369&objectId=news018,0005548118
            var commentsResponse = await _httpClient.GetAsync($"{BASE_URL}{COMMENT_PATH}?resultType=MAP&ticket=news&lang=ko&country=KR&{objectParams}");
            commentsResponse.EnsureSuccessStatusCode();

            var rawCommentsBytes = await commentsResponse.Content.ReadAsByteArrayAsync();
            var rawComments = DecodeEucKr(rawCommentsBytes);
            AddComments(articles, rawComments);

            return articles;
        }

        private IEnumerable<Article> ParseArticles(string raw, NewsType type)
        {
            //{
            //    "itemList": [],
            //    "dateList": [],
            //    "pagerInfo": {
            //        "type": "default",
            //        "totalRows": 12674,
            //        "pageSize": 20,
            //        "indexSize": 10,
            //        "page": 1,
            //        "requestURI": "/main/mainNews.naver",
            //        "requestMethod": "POST",
            //        "queryString": "sid1=102&firstLoad=Y",
            //        "startRownum": 1,
            //        "endRownum": 20,
            //        "totalPages": 634,
            //        "firstPage": 1,
            //        "lastPage": 10,
            //        "prevPage": 0,
            //        "nextPage": 2
            //    },
            //    "currentDate": null,
            //    "airsResult": "{\"success\":.."
            //}
            //{
            //  "success": true,
            //  "message": "OK",
            //  "resultCode": 200,
            //  "result": {
            //    "102": [
            //      {
            //        "sectionId": "102",
            //        "gdid": "880000FF_000000000000000005055239",
            //        "articleId": "0005055239",
            //        "officeId": "014",
            //        "officeName": "파이낸셜뉴스",
            //        "title": "분당 흉기난동범 최원종 \"피해자분께 죄송\"...여전히 '스토킹 피해' 주장",
            //        "summary": "'분당 흉기 난동 사건'의 범인 최원종(22)이 10일 피해자들에게 \"정말 죄송하다\"며 사과했다. 하지만 자신은 여전히 \"스토킹 집단의 피해지로, 범행 당일에도 괴롭힘 당하고 있었다\"고 말하며 피해망상 증상을 보였다",
            //        "serviceTime": 1691628303000,
            //        "imageUrl": "https://mimgnews.pstatic.net/image/origin/014/2023/08/10/5055239.jpg",
            //        "type": "1",
            //        "sessionId": "pbt8MTcylIWxjlTg",
            //        "modelVersion": "news_sec_v2.0",
            //        "airsServiceType": "DEFAULT"
            //      },
            //      {
            //        "sectionId": "102",
            //        "gdid": "880000C1_000000000000000000829845",
            //        "articleId": "0000829845",
            //        "officeId": "088",
            //        "officeName": "매일신문",
            //        "title": "[속보] 72년 만에 한반도 관통 태풍 '카눈' 오전 9시 20분 경남 거제 상륙",
            //        "summary": "기상청은 제6호 태풍 '카눈'이 10일 오전 9시 20분 기준 경남 거제 부근으로 상륙했다고 밝혔다. 카눈은 한반도 상륙 직전까지만 해도 강도가 '강'을 유지했지만 상륙하면서 세력이 약화, '중' 강도로 내려앉았을 ",
            //        "serviceTime": 1691628301000,
            //        "imageUrl": "https://mimgnews.pstatic.net/image/origin/088/2023/08/10/829845.jpg",
            //        "type": "1",
            //        "sessionId": "pbt8MTcylIWxjlTg",
            //        "modelVersion": "news_sec_v2.0",
            //        "airsServiceType": "DEFAULT"
            //      },
            //    ]
            //  },
            //  "currentPage": 1,
            //  "totalPage": 634,
            //  "totalCount": 12674
            //}
            var root = JsonNode.Parse(raw);
            var airsResultRaw = root["airsResult"].ToString();
            var airsResult = JsonNode.Parse(airsResultRaw);
            var articleArray = (JsonArray)airsResult["result"][((int)type).ToString()];

            var articles = articleArray
                .Select(n => new Article
                {
                    Type = type,
                    ArticleId = n["articleId"]?.ToString(),
                    OfficeId = n["officeId"]?.ToString(),
                    Title = n["title"]?.ToString(),
                    Summary = n["summary"]?.ToString(),
                    ImageUrl = n["imageUrl"]?.ToString(),
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(n["serviceTime"].ToString())).UtcDateTime,
                    // /586/0000062631?sid=102
                    ArticleUrl = $"{ARTICLE_URL}/{n["officeId"]?.ToString()}/{n["articleId"]?.ToString()}?sid={(int)type}"
                });

            return articles;
        }
    }
}