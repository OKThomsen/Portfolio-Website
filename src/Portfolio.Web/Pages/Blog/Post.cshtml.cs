using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Portfolio.Web.Data;
using Portfolio.Web.Models;

namespace Portfolio.Web.Pages.Blog;

public class PostModel(PortfolioDbContext db) : PageModel
{
    public BlogPost Post { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var post = await db.BlogPosts
            .Include(p => p.Tags.OrderBy(t => t.SortOrder))
            .Include(p => p.RelatedProject)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (post is null)
        {
            return NotFound();
        }

        Post = post;
        return Page();
    }
}
