public class PostPreviewDTO
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CustomUrl { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? Tags { get; set; }
}