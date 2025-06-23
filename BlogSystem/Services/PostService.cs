using Mapster;

public class PostService
{
    public Post CreatePost(PostCreateDTO dto, string authorUsername)
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

    private string Slugify(string title)
    {
        return title.ToLower().Replace(" ", "-");
    }

    private int GeneratePostId()
    {
        return PostStore.Posts.OrderByDescending(post => post.Id).FirstOrDefault().Id + 1;
    }
}
