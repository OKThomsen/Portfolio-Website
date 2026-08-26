namespace Portfolio.Web.Models;

public class Skill
{
    public int Id { get; set; }
    public int SkillCategoryId { get; set; }
    public SkillCategory? SkillCategory { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
}
