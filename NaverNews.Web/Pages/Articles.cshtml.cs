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

            Response.Cookies.Append(nameof(Count), Count.ToString());
            Response.Cookies.Append(nameof(MinimumTotal), MinimumTotal.ToString());
            Response.Cookies.Append(nameof(OlderThan), OlderThan.ToString());
        }

        public async Task<IActionResult> OnPostAutoPostAsync(string id)
        {
            try
            {
                await _articleService.AutoPost(id);

                var count = Convert.ToInt16(Request.Cookies[nameof(Count)]);
                var minimumTotal = Convert.ToInt16(Request.Cookies[nameof(MinimumTotal)]);
                var olderThan = Convert.ToDateTime(Request.Cookies[nameof(OlderThan)]);

                return RedirectToPage(new { Count = count, MinimumTotal = minimumTotal, OlderThan = olderThan });
            }
            catch (ArticleNotFoundException e)
            {
                return NotFound();
            }
        }
    }
}