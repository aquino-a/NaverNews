using System.ComponentModel.DataAnnotations;

namespace NaverNews.Core
{
    public class SearchResult
    {
        [Key]
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public int Count { get; set; }
    }
}