using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NaverNews.Web.Models;

namespace NaverNews.Web.Data
{
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options, operationalStoreOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder
                .Entity<IdentityUser>()
                .HasData(
                    new IdentityUser
                    {
                        Id = "04fc2e85-06d3-4be1-89fa-a87579465e85",
                        UserName = "aquino@beancorp.com",
                        NormalizedUserName = "AQUINO@BEANCORP.COM",
                        Email = "aquino@beancorp.com",
                        NormalizedEmail = "AQUINO@BEANCORP.COM",
                        EmailConfirmed = true,
                        PasswordHash = "AQAAAAIAAYagAAAAEMLjSL5I8JiyQD0peAP1n8FGncPCFZT71fuQni6DDR/bH7Lv7iZVsY2CcOjq69HY3Q==",
                        SecurityStamp = "NAJOOAIGEZII4EMEAOLSGJYEIWLTJG6",
                        ConcurrencyStamp = "49f14206-464f-4f8e-9690-a78c9a07995b",
                        PhoneNumber = null,
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = false,
                        AccessFailedCount = 0
                    });
        }
    }
}