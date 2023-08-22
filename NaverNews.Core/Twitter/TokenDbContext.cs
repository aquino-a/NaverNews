using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverNews.Core
{
    public class TokenDbContext : DbContext
    {
        public TokenDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Tokens> Tokens { get; set; }
    }
}
