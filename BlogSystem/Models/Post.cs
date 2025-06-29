public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }  // null = draft
    public DateTime? ModifiedAt { get; set; }
    public string CustomUrl { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public PostStatus Status { get; set; }
    public MetaData Metadata { get; set; } = new();
    public List<string> Assets { get; set; } = new();

}