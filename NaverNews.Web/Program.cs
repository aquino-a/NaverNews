using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Moq;
using NaverNews.Core;
using NaverNews.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration["Cosmos-Connection"] ?? throw new InvalidOperationException("Connection string 'Cosmos-Connection' not found.");
var twitterClientId = builder.Configuration["Twitter:clientId"] ?? throw new InvalidOperationException("Twitter client id 'Twitter:clientId' not found.");
var twitterTokens = new Tokens
{
    Access = builder.Configuration["Twitter:accessToken"] ?? throw new InvalidOperationException("Twitter refresh token 'Twitter:accessToken' not found."),
    Refresh = builder.Configuration["Twitter:refreshToken"] ?? throw new InvalidOperationException("Twitter access token 'Twitter:refreshToken' not found.")
};

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<SearchAutoPostService>();
    builder.Services.AddScoped<IArticleService, ArticleService>();
}
else
{
    builder.Services.AddScoped<IArticleService>(sp =>
    {
        var articeService = new Mock<IArticleService>();
        articeService.Setup(a => a.GetByTimeAndTotal(DateTime.Now, 10, 10)).Returns(Article.FakeArticles);

        return articeService.Object;
    });
}

builder.Services.AddDbContext<ArticleDbContext>(options => options.UseCosmos(connectionString, "news"));
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<NaverClient>();
builder.Services.AddSingleton<TwitterClient>((sp) =>
{
    var tc = new TwitterClient(twitterClientId, sp.GetRequiredService<HttpClient>());
    tc.Tokens = twitterTokens;

    return tc;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var articleContext = scope.ServiceProvider.GetRequiredService<ArticleDbContext>();
    await articleContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
