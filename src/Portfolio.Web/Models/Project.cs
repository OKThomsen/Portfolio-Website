namespace Portfolio.Web.Models;

public class Project
{
    public int Id { get; set; }
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public string? Purpose { get; set; }
    public string? Contribution { get; set; }
    public string? EngineeringNotes { get; set; }
    public string? Outcome { get; set; }
    public string? RepositoryUrl { get; set; }
    public string? DemoUrl { get; set; }
    public bool HasDetailPage { get; set; }
    public bool Featured { get; set; }
    public int SortOrder { get; set; }

    public List<ProjectTechnology> Technologies { get; set; } = [];
}
