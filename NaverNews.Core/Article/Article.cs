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

        public string TranslatedSummary { get; set; }
        public string TwitterUrl { get; set; }
        public NewsType Type { get; internal set; }
    }
}