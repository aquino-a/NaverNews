using Microsoft.AspNetCore.Mvc;
using NaverNews.Core;

namespace NaverNews.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ILogger<ArticleController> _logger;

        public ArticleController(IArticleService articleService, ILogger<ArticleController> logger)
        {
            _articleService = articleService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetArticles([FromQuery] DateTime olderThanUtc,
                                                     [FromQuery] int minCount,
                                                     [FromQuery] int count)
        {
            var articles = _articleService.GetByTimeAndTotal(olderThanUtc, minCount, count).ToArray();

            return Ok(articles);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetSearchResults([FromQuery] DateTime olderThanUtc,
                                                          [FromQuery] int count = 10)
        {
            var searchResults = await _articleService.GetSearchResults(olderThanUtc, count);

            return Ok(searchResults.ToArray());
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string articleId)
        {
            var summary = await _articleService.GetSummary(articleId);

            return Ok(summary);
        }

        [HttpGet("text")]
        public async Task<IActionResult> GetText([FromQuery] string articleId)
        {
            var text = await _articleService.GetArticleText(articleId);

            return Ok(text);
        }

        [HttpPost("post")]
        public async Task<IActionResult> Post([FromQuery] string articleId)
        {
            var id = await _articleService.Post(articleId);

            return Ok(id);
        }

        [HttpPost("search")]
        public async Task<IActionResult> StartSearch()
        {
            Task.Factory.StartNew(async () => await _articleService.SearchArticles(NewsType.Society, 20));

            return Ok();
        }
    }
}