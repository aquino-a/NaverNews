using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NaverNews.Core;

namespace NaverNews.Web.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IArticleService _articleService;

        public SearchModel(IArticleService articleService)
        {
            _articleService = articleService;
        }

        public IEnumerable<SearchResult> SearchResults { get; set; }

        public async Task OnGet([FromQuery] int count = 10)
        {
            SearchResults = (await _articleService.GetSearchResults(DateTime.UtcNow, count))
                .ToArray();
        }

        public async Task<IActionResult> OnPost([FromForm] int pageCount = 20)
        {
            await _articleService.SearchArticles(NewsType.Society, pageCount);

            return RedirectToPage();
        }
    }
}