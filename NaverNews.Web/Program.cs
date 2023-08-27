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
var trimLength = builder.Configuration.GetValue<int>("Article:trimLength");

builder.Logging.AddAzureWebAppDiagnostics().AddFilter("Microsoft", LogLevel.Error);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IArticleService, ArticleService>(sp =>
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
    articleService.TrimLength = trimLength;

    return articleService;
});

builder.Services.AddDbContext<ArticleDbContext>(options => options.UseCosmos(connectionString, "news"), ServiceLifetime.Scoped);
builder.Services.AddDbContext<TokenDbContext>(options => options.UseCosmos(connectionString, "news"), ServiceLifetime.Transient, ServiceLifetime.Transient);
builder.Services.AddTransient<HttpClient>();
builder.Services.AddScoped<NaverClient>();
builder.Services.AddScoped<IChatGptService, ChatGptService>(sp => new ChatGptService(sp.GetRequiredService<HttpClient>(), chatGptApiKey));
builder.Services.AddSingleton<TokenService>(sp =>
{
    var tokenService = new TokenService(twitterClientId,
                                        twitterClientSecret,
                                        sp.GetRequiredService<TokenDbContext>(),
                                        sp.GetRequiredService<HttpClient>(),
                                        sp.GetRequiredService<ILogger<TokenService>>());
    tokenService.Load().GetAwaiter().GetResult();

    return tokenService;
});
builder.Services.AddScoped<TwitterClient>((sp) =>
{
    var tc = new TwitterClient(sp.GetRequiredService<TokenService>(),
                               sp.GetRequiredService<HttpClient>(),
                               sp.GetRequiredService<ILogger<TwitterClient>>());
    return tc;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var articleContext = scope.ServiceProvider.GetRequiredService<ArticleDbContext>();
    await articleContext.Database.EnsureCreatedAsync();

    var tokenContext = scope.ServiceProvider.GetRequiredService<TokenDbContext>();
    await tokenContext.Database.EnsureCreatedAsync();
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