using Microsoft.EntityFrameworkCore;
using NaverNews.Core;
using NaverNews.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration["Cosmos-Connection"] ?? throw new InvalidOperationException("Connection string 'Cosmos-Connection' not found.");
var chatGptApiKey = builder.Configuration["ChatGpt:apiKey"] ?? throw new InvalidOperationException("ChatGpt Api key 'ChatGpt:apiKey' not found.");
var twitterClientId = builder.Configuration["Twitter:clientId"] ?? throw new InvalidOperationException("Twitter client id 'Twitter:clientId' not found.");
var twitterClientSecret = builder.Configuration["Twitter:clientSecret"] ?? throw new InvalidOperationException("Twitter client secret 'Twitter:clientSecret' not found.");
var twitterTokens = new Tokens
{
    Access = builder.Configuration["Twitter:accessToken"] ?? throw new InvalidOperationException("Twitter refresh token 'Twitter:accessToken' not found."),
    Refresh = builder.Configuration["Twitter:refreshToken"] ?? throw new InvalidOperationException("Twitter access token 'Twitter:refreshToken' not found.")
};

var engagementMinimum = builder.Configuration.GetValue<int>("Article:engagementMinimum");
var searchPageCount = builder.Configuration.GetValue<int>("Article:searchPageCount");
var skipThreshhold = builder.Configuration.GetValue<int>("Article:skipThreshhold");

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddHostedService<SearchAutoPostService>();
builder.Services.AddSingleton<IArticleService, ArticleService>(sp =>
{
    var articleService = new ArticleService(
        sp.GetRequiredService<NaverClient>(),
        sp.GetRequiredService<IChatGptService>(),
        sp.GetRequiredService<TwitterClient>(),
        sp.GetRequiredService<ArticleDbContext>(),
        sp.GetRequiredService<ILogger<ArticleService>>());

    articleService.EngagementMinimum = engagementMinimum;
    articleService.SearchPageCount = searchPageCount;
    articleService.SkipThreshhold = skipThreshhold;

    return articleService;
});

builder.Services.AddDbContext<ArticleDbContext>(options => options.UseCosmos(connectionString, "news"), ServiceLifetime.Singleton);
builder.Services.AddTransient<HttpClient>();
builder.Services.AddSingleton<NaverClient>();
builder.Services.AddSingleton<IChatGptService, ChatGptService>(sp => new ChatGptService(sp.GetRequiredService<HttpClient>(), chatGptApiKey));
builder.Services.AddSingleton<TwitterClient>((sp) =>
{
    var tc = new TwitterClient(twitterClientId, twitterClientSecret, sp.GetRequiredService<HttpClient>());
    tc.Tokens = twitterTokens;

    return tc;
});

var app = builder.Build();

var articleContext = app.Services.GetRequiredService<ArticleDbContext>();
await articleContext.Database.EnsureCreatedAsync();

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