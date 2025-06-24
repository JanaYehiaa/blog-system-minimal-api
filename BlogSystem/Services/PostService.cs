
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
        post.CustomUrl = dto.CustomUrl;
        post.Status = PostStatus.Draft;
        post.Metadata = dto.Metadata;
        post.Assets = dto.Assets;
        return post;
    }

    public PostViewDTO? GetPostViewDTOByCustomUrl(string customUrl)
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

    public bool DeletePostByCustomUrl(string customUrl)
    {
        var post = PostStore.Posts.FirstOrDefault(p => p.CustomUrl == customUrl);
        if (post is not null)
        {
            PostStore.Posts.Remove(post);
            return true;
        }
        return false;
    }

    public Post? UpdatePost(PostUpdateDTO dto, string customUrl)
    {
        var post = PostStore.Posts.FirstOrDefault(p => p.CustomUrl == customUrl);
        if (post is null) return null;

        post.Title = dto.Title;
        post.Description = dto.Description;
        post.Body = dto.Body;
        post.Slug = Slugify(dto.Title);
        post.ModifiedAt = DateTime.UtcNow;
        post.Status = dto.Status; //restrict later with JWT-based roles
        post.Metadata = dto.Metadata;
        post.Assets = dto.Assets;

        return post;
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
