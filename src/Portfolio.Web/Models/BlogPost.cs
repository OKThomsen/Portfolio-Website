namespace Portfolio.Web.Models;

public class BlogPost
{
    public int Id { get; set; }
    public required string Slug { get; set; }
    public required string Title { get; set; }
    public required string Excerpt { get; set; }
    public required string Body { get; set; }
    public DateOnly PublishedOn { get; set; }

    public int? RelatedProjectId { get; set; }
    public Project? RelatedProject { get; set; }

    public List<BlogPostTag> Tags { get; set; } = [];
}
