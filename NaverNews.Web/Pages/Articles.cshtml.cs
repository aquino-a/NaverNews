using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

        public void OnGet()
        {
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
