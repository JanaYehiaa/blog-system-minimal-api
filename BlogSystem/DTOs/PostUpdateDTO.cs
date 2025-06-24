public class PostUpdateDTO
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public PostStatus Status { get; set; }
    public MetaData Metadata { get; set; } = new();
    public List<string> Assets { get; set; } = new();

}