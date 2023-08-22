using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NaverNews.Core;

namespace NaverNews.Web.Pages
{
    public class ArticlesModel : PageModel
    {
        private readonly IArticleService _articleService;

        public ArticlesModel(IArticleService articleService)
        {
            _articleService = articleService;
        }

        public IEnumerable<Article> Articles { get; private set; }
        public int Count { get; private set; }
        public int MinimumTotal { get; private set; }
        public DateTime OlderThan { get; private set; }

        public void OnGet([FromQuery] DateTime olderThan,
                          [FromQuery] int minimumTotal = 500,
                          [FromQuery] int count = 5)
        {
            if (olderThan == default(DateTime))
            {
                olderThan = DateTime.UtcNow;
            }

            Articles = _articleService.GetByTimeAndTotal(olderThan, minimumTotal, count).ToArray();

            Count = count;
            MinimumTotal = minimumTotal;
            OlderThan = olderThan;
        }

        public async Task<IActionResult> OnPostAutoPostAsync(string id)
        {
            try
            {
                await _articleService.AutoPost(id);

                return RedirectToPage();
            }
            catch (ArticleNotFoundException e)
            {
                return NotFound();
            }
        }
    }
}