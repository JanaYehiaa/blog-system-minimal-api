
public class PostService
{
    public Post CreatePost(PostCreateDTO dto, string authorUsername, string authorId) 
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
        post.AuthorId = authorId;
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

        if (!string.IsNullOrWhiteSpace(dto.Title) && dto.Title != post.Title)
        {
            post.Title = dto.Title;
            post.Slug = Slugify(dto.Title);
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
            post.Description = dto.Description;

        if (!string.IsNullOrWhiteSpace(dto.Body))
            post.Body = dto.Body;

        if (dto.Status.HasValue)
            post.Status = dto.Status.Value;

        if (dto.Metadata?.Tags?.Any() == true)
            post.Metadata.Tags = dto.Metadata.Tags;

        if (dto.Metadata?.Categories?.Any() == true)
            post.Metadata.Categories = dto.Metadata.Categories;

        if (dto.Assets?.Any() == true)
            post.Assets = dto.Assets;

        post.ModifiedAt = DateTime.UtcNow;

        return post;
    }

    public Post? PublishPost(string customUrl, DateTime? publishAt)
    {
        var post = PostStore.Posts.FirstOrDefault(p => p.CustomUrl == customUrl);
        if (post == null)
        {
        return null;
        } 
        post.Status = PostStatus.Published;
        post.PublishedAt = publishAt ?? DateTime.UtcNow;
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
