using Microsoft.EntityFrameworkCore;
using Portfolio.Web.Models;

namespace Portfolio.Web.Data;

public class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTechnology> ProjectTechnologies => Set<ProjectTechnology>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasIndex(p => p.Slug)
            .IsUnique();

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Technologies)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SkillCategory>()
            .HasMany(c => c.Skills)
            .WithOne(s => s.SkillCategory)
            .HasForeignKey(s => s.SkillCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
