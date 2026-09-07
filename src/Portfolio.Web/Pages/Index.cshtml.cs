using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Portfolio.Web.Data;
using Portfolio.Web.Models;

namespace Portfolio.Web.Pages;

public class IndexModel(PortfolioDbContext db) : PageModel
{
    public List<SkillCategory> SkillCategories { get; set; } = [];
    public List<Project> FeaturedProjects { get; set; } = [];

    // Maps a skill name to an icon file in wwwroot/img/tech (without extension).
    // Icon choice is presentation, so it lives here rather than in the database.
    // Names not present here render with a neutral placeholder glyph.
    private static readonly Dictionary<string, string> IconBySkill = new()
    {
        ["C#"] = "csharp",
        ["Python"] = "python",
        ["Java"] = "java",
        ["SQL"] = "sql",
        ["HTML/CSS"] = "htmlcss",
        ["ASP.NET Core"] = "aspnet",
        ["Entity Framework Core"] = "efcore",
        ["REST APIs"] = "restapi",
        ["MQTT"] = "mqtt",
        ["CI/CD"] = "cicd",
        ["Linux"] = "linux",
        ["Azure"] = "azure",
        ["Google Cloud"] = "googlecloud",
        ["Node.js"] = "nodejs",
        ["React"] = "react",
        ["SQL Server"] = "sqlserver",
        ["MySQL"] = "mysql",
        ["PostgreSQL"] = "postgresql",
        ["MongoDB"] = "mongodb",
        ["Redis"] = "redis",
        ["Docker"] = "docker",
        ["Kubernetes"] = "kubernetes",
        ["Git"] = "git",
        ["Apache Kafka"] = "kafka",
        ["RabbitMQ"] = "rabbitmq",
    };

    public static string? IconFor(string skillName) =>
        IconBySkill.TryGetValue(skillName, out var icon) ? icon : null;

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
