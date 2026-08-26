namespace Portfolio.Web.Models;

public class SkillCategory
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }

    public List<Skill> Skills { get; set; } = [];
}
