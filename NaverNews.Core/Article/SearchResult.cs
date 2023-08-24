using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NaverNews.Core
{
    [Index(nameof(EndTime))]
    public class SearchResult
    {
        public int Count { get; set; }
        public DateTime EndTime { get; set; }

        [Key]
        public DateTime StartTime { get; set; }
    }
}