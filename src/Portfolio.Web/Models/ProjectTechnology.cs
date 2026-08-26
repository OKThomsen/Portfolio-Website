namespace Portfolio.Web.Models;

public class ProjectTechnology
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
}
