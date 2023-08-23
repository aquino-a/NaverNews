using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NaverNews.Core;
using System;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(NaverNews.Function.Startup))]

namespace NaverNews.Function
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("Cosmos-Connection") ?? throw new InvalidOperationException("Connection string 'Cosmos-Connection' not found.");
            var chatGptApiKey = Environment.GetEnvironmentVariable("ChatGpt:apiKey") ?? throw new InvalidOperationException("ChatGpt Api key 'ChatGpt:apiKey' not found.");
            var twitterClientId = Environment.GetEnvironmentVariable("Twitter:clientId") ?? throw new InvalidOperationException("Twitter client id 'Twitter:clientId' not found.");
            var twitterClientSecret = Environment.GetEnvironmentVariable("Twitter:clientSecret") ?? throw new InvalidOperationException("Twitter client secret 'Twitter:clientSecret' not found.");

            var engagementMinimum = Convert.ToInt32(Environment.GetEnvironmentVariable("Article:engagementMinimum"));
            var searchPageCount = Convert.ToInt32(Environment.GetEnvironmentVariable("Article:searchPageCount"));
            var skipThreshhold = Convert.ToInt32(Environment.GetEnvironmentVariable("Article:skipThreshhold"));

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
        }
    }
}