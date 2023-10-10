using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverNews.Core
{
    public class ArticleDbContext : DbContext
    {
        private static readonly int DELETE_LIMIT = 1_000;
        public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<SearchResult> SearchResults { get; set; }

        public async Task DeleteOld()
        {
            var cutOff = DateTime.Now.AddDays(-30);

            var deleteArticles = await Articles
                .Where(a => a.Time <= cutOff)
                .Where(a => a.TwitterId == null)
                .Take(DELETE_LIMIT)
                .ToListAsync();

            Articles.RemoveRange(deleteArticles);

            var deleteSearchResults = await SearchResults
                .Where(s => s.StartTime <= cutOff)
                .Take(DELETE_LIMIT)
                .ToListAsync();

            SearchResults.RemoveRange(deleteSearchResults);

            await SaveChangesAsync();
        }
    }
}
