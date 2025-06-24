
public class PostService
{
    public Post CreatePost(PostCreateDTO dto, string authorUsername = "admin") //hardcoded for now, remove when JWT is done
    {
        Post post = new();

        post.Id = GeneratePostId();
        post.Title = dto.Title;
        post.Slug = Slugify(dto.Title);
        post.Description = dto.Description;
        post.Body = dto.Body;
        post.CreatedAt = DateTime.UtcNow;
        post.PublishedAt = null;
        post.ModifiedAt = null;
        post.AuthorUsername = authorUsername;
        post.Status = PostStatus.Draft;
        post.Metadata = dto.Metadata;
        post.Assets = dto.Assets;
        return post;
    }

    public PostViewDTO? GetPostByCustomUrl(string customUrl)
    {
        var post = PostStore.Posts.FirstOrDefault(p => p.CustomUrl == customUrl);
        if (post is null) return null;

        return new PostViewDTO
        {
            Title = post.Title,
            Description = post.Description,
            Body = post.Body,
            PublishedAt = post.PublishedAt,
            CustomUrl = post.CustomUrl,
            AuthorUsername = post.AuthorUsername,
            Metadata = post.Metadata,
            Assets = post.Assets
        };
    }

    private string Slugify(string title)
    {
        return title.ToLower().Replace(" ", "-");
    }

    private int GeneratePostId()
    {
        if (PostStore.Posts.Count == 0)
        {
            return 1;
        }
        else
        {
            var lastPost = PostStore.Posts.OrderByDescending(post => post.Id).FirstOrDefault();
            return (lastPost?.Id ?? 0) + 1;
        }
    }
}
