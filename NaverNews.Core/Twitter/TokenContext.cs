using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaverNews.Core.Twitter
{
    public class TokenContext : DbContext
    {
        public TokenContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Tokens> Tokens { get; set; }
    }
}
