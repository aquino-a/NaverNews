using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using NaverNews.Core;

namespace NaverNews.Function
{
    public class AutoPostTimer
    {
        private readonly IArticleService _articleService;

        [FunctionName("AutoPostTimer")]
        public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger logger)
        {
            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            try
            {
                await _articleService.AutoPost();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Problem autoposting articles.");
            }
        }
    }
}
