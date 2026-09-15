using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Portfolio.Web.Data;
using Portfolio.Web.Models;

namespace Portfolio.Web.Pages.Blog;

public class IndexModel(PortfolioDbContext db) : PageModel
{
    public List<BlogPost> Posts { get; set; } = [];

    public async Task OnGetAsync()
    {
        Posts = await db.BlogPosts
            .Include(p => p.Tags.OrderBy(t => t.SortOrder))
            .OrderByDescending(p => p.PublishedOn)
            .ToListAsync();
    }
}
