public class PostStore
{
    public static List<Post> Posts = new List<Post>
    {
        new Post
        {
            Id = 1,
            Title = "Test Post",
            Slug = "test-post",
            Description = "This is a test post",
            Body = "Lorem ipsum dolor sit amet.",
            CreatedAt = DateTime.UtcNow,
            AuthorUsername = "admin",
            Status = PostStatus.Draft,
            Metadata = new()
            {
                Tags = ["test", "blog"],
                Categories = ["General"]
            }
        }
    };
}
