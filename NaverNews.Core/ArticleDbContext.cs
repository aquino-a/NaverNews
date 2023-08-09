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
        public DbSet<Article> Articles { get; set; }
    }
}
