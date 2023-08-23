using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NaverNews.Core;

namespace NaverNews.Function
{
	public class AutoPostTimer
	{
		private readonly IArticleService _articleService;

		public AutoPostTimer(IArticleService articleService)
		{
			_articleService = articleService;
		}

		[Function("AutoPostTimer")]
		public async Task Run([TimerTrigger("0 */1 * * * *")] ILogger logger)
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
