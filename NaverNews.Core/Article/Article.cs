using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NaverNews.Core
{
    [Index(nameof(Time))]
    public class Article
    {
        public string ArticleId { get; set; }
        public string ArticleUrl { get; set; }
        public int CommentCount { get; internal set; }
        public string ImageUrl { get; internal set; }

        public bool IsOnTwitter { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string ObjectName
        {
            get
            {
                return $"news{OfficeId},{ArticleId}";
            }
        }

        public string OfficeId { get; internal set; }
        public int ReplyCount { get; internal set; }
        public string Summary { get; internal set; }
        public string Text { get; internal set; }
        public DateTime Time { get; internal set; }
        public string Title { get; internal set; }

        public int Total
        {
            get
            {
                return CommentCount + ReplyCount;
            }
        }

        public string TranslatedSummary { get; internal set; }
        public string TwitterId { get; internal set; }
        public string TwitterUrl { get; set; }
        public NewsType Type { get; internal set; }

        public static IEnumerable<Article> FakeArticles()
        {
            return new[]
            {
                new Article
                {
                    ArticleId = "0006992991",
                    ImageUrl = "https://mimgnews.pstatic.net/image/origin/421/2023/08/16/6992991.jpg",
                    Time = DateTime.Parse("2023-08-16T05:50:30Z"),
                    Title ="\"떨이상품 오픈런 1시간이면 동나…올 추석엔 차례상 다이어트\"",
                    CommentCount = 0,
                    ReplyCount = 0,
                    Summary = "유민주 김예원 기자 = #주부 A씨(53)는 마트가 문을 열자마자 유통기한이 얼마남지 않은 '떨이상품'이 진열된 매대로 향했다. 2~3명의 주부도 재빠르게 카트를 밀며 뒤따랐다. 채소와 과일 가격이 치솟으면서 나타난"
                },
                new Article
                {
                    ArticleId = "0003385308",
                    ImageUrl = "https://mimgnews.pstatic.net/image/origin/081/2023/08/16/3385308.jpg",
                    Time = DateTime.Parse("023-08-16T05:47:01Z"),
                    Title ="장미란 차관, 진천선수촌 찾았다…“더 꼼꼼히 챙기겠다”",
                    CommentCount = 1,
                    ReplyCount = 0,
                    Summary = "장미란 문화체육관광부 2차관이 16일 충북 진천 국가대표 선수촌을 방문해 2022 항저우 하계아시안게임 준비에 매진하는 국가대표 후배들을 격려했다. 장미란 차관은 이날 선수들의 훈련 상황을 보고 받은 뒤 “진천선수촌"
                },
                new Article
                {
                    ArticleId = "0006992991",
                    ImageUrl = "https://mimgnews.pstatic.net/image/origin/421/2023/08/16/6992991.jpg",
                    Time = DateTime.Parse("2023-08-16T05:50:30Z"),
                    Title ="\"떨이상품 오픈런 1시간이면 동나…올 추석엔 차례상 다이어트\"",
                    CommentCount = 0,
                    ReplyCount = 0,
                    Summary = "유민주 김예원 기자 = #주부 A씨(53)는 마트가 문을 열자마자 유통기한이 얼마남지 않은 '떨이상품'이 진열된 매대로 향했다. 2~3명의 주부도 재빠르게 카트를 밀며 뒤따랐다. 채소와 과일 가격이 치솟으면서 나타난"
                },
                new Article
                {
                    ArticleId = "0006992991",
                    ImageUrl = "https://mimgnews.pstatic.net/image/origin/421/2023/08/16/6992991.jpg",
                    Time = DateTime.Parse("2023-08-16T05:50:30Z"),
                    Title ="\"떨이상품 오픈런 1시간이면 동나…올 추석엔 차례상 다이어트\"",
                    CommentCount = 0,
                    ReplyCount = 0,
                    Summary = "유민주 김예원 기자 = #주부 A씨(53)는 마트가 문을 열자마자 유통기한이 얼마남지 않은 '떨이상품'이 진열된 매대로 향했다. 2~3명의 주부도 재빠르게 카트를 밀며 뒤따랐다. 채소와 과일 가격이 치솟으면서 나타난"
                },

            };
        }
    }
}