
using System.Threading.Tasks;

public class PostService
{
    public async Task<Post> CreatePost(PostCreateDTO dto, string authorUsername, string authorId)
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

        await FileStorageHandler.SavePost(post);

        foreach (var tag in post.Metadata.Tags)
        {
            FileStorageHandler.SaveTagIfNotExists(tag);
        }

        foreach (var category in post.Metadata.Categories)
        {
            FileStorageHandler.SaveCategoryIfNotExists(category);
        }
        return post;
    }

    public async Task<PostViewDTO?> GetPostViewDTOByCustomUrl(string customUrl)
    {
        var post = await FileStorageHandler.GetPostByCustomUrl(customUrl);
        if (post is null) return null;

        return FileStorageHandler.ReadPostFromFile(post.Slug);

    }

    public async Task<bool> DeletePostByCustomUrl(string customUrl)
    {
        var post = await FileStorageHandler.GetPostByCustomUrl(customUrl); ;
        if (post is not null)
        {
            return FileStorageHandler.DeletePost(post);

        }
        return false;
    }

    public async Task<PostViewDTO?> UpdatePost(PostUpdateDTO dto, string customUrl)
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
        {
            post.Metadata.Tags = dto.Metadata.Tags;
            foreach (var tag in post.Metadata.Tags)
            {
                FileStorageHandler.SaveTagIfNotExists(tag);
            }
        }

        if (dto.Metadata?.Categories?.Any() == true)
        {
            post.Metadata.Categories = dto.Metadata.Categories;
            foreach (var category in post.Metadata.Categories)
            {
                FileStorageHandler.SaveCategoryIfNotExists(category);
            }
        }

        if (dto.Assets?.Any() == true)
            post.Assets = dto.Assets;

        post.ModifiedAt = DateTime.UtcNow;

        await FileStorageHandler.SavePost(post);
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

    public async Task<Post?> PublishPost(string customUrl, DateTime? publishAt)
    {
        var post = await FileStorageHandler.GetPostByCustomUrl(customUrl);
        if (post == null)
        {
            return null;
        }
        post.Status = PostStatus.Published;
        post.PublishedAt = publishAt ?? DateTime.UtcNow;
        await FileStorageHandler.SavePost(post);
        return post;
    }

    public async Task<PagedResult<PostPreviewDTO>> GetPaginatedSummaries(int page, int pageSize, string? search)
    {
        var all = await FileStorageHandler.GetAllPublishedPostSummaries();

        if (!string.IsNullOrWhiteSpace(search))
        {
            all = all
                .Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var total = all.Count;
        var paged = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<PostPreviewDTO>
        {
            Items = paged,
            TotalCount = total
        };
    }

    public async Task<PagedResult<PostPreviewDTO>> GetPaginatedPostsByCategory(string category, int page, int pageSize)
    {
        var all = await FileStorageHandler.GetAllPublishedPostSummaries();

        var filtered = all.Where(p => p.Categories != null && p.Categories.Contains(category, StringComparer.OrdinalIgnoreCase)).ToList();

        var total = filtered.Count;

        var paged = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<PostPreviewDTO>
        {
            Items = paged,
            TotalCount = total
        };
    }

    public async Task<PagedResult<PostPreviewDTO>> GetPaginatedPostsByTagSearch(string tagSearch, int page, int pageSize)
    {
        var all = await FileStorageHandler.GetAllPublishedPostSummaries();

        var filtered = all
            .Where(p => p.Tags != null && p.Tags.Any(t => t.Contains(tagSearch, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var total = filtered.Count;

        var paged = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<PostPreviewDTO>
        {
            Items = paged,
            TotalCount = total
        };
    }

    public async Task<PagedResult<PostPreviewDTO>> GetPostsByCategory(string category, int page, int pageSize)
        => await GetPaginatedPostsByCategory(category, page, pageSize);

    public async Task<PagedResult<PostPreviewDTO>> GetPostsByTag(string tag, int page, int pageSize)
        => await GetPaginatedPostsByTagSearch(tag, page, pageSize);

    public async Task<List<string>> GetAllCategories()
        => await FileStorageHandler.GetAllCategories();



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
