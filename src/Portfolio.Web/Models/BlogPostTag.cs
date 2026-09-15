namespace Portfolio.Web.Models;

public class BlogPostTag
{
    public int Id { get; set; }
    public int BlogPostId { get; set; }
    public BlogPost? BlogPost { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
}
