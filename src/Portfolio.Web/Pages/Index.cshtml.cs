using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Portfolio.Web.Data;
using Portfolio.Web.Models;

namespace Portfolio.Web.Pages;

public class IndexModel(PortfolioDbContext db) : PageModel
{
    public List<SkillCategory> SkillCategories { get; set; } = [];
    public List<Project> FeaturedProjects { get; set; } = [];

    public async Task OnGetAsync()
    {
        SkillCategories = await db.SkillCategories
            .Include(c => c.Skills.OrderBy(s => s.SortOrder))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        FeaturedProjects = await db.Projects
            .Include(p => p.Technologies.OrderBy(t => t.SortOrder))
            .Where(p => p.Featured)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();
    }
}
