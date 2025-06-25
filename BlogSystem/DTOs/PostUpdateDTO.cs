public class PostUpdateDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Body { get; set; }
    public PostStatus? Status { get; set; }
    public MetaData? Metadata { get; set; }
    public List<string>? Assets { get; set; }
}
