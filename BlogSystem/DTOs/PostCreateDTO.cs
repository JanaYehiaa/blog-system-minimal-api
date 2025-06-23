public class PostCreateDTO
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string CustomUrl { get; set; } = string.Empty;
    public MetaData Metadata { get; set; } = new();
    public List<string> Assets { get; set; } = new();
}