using System.Text.Json;

public static class FileStorageHandler
{
    private static readonly string postBasePath = Path.Combine("Content", "Posts");
    private static readonly string userBasePath = Path.Combine("Content", "Users");
    private static readonly string categoryBasePath = Path.Combine("Content", "Categories");
    private static readonly string tagBasePath = Path.Combine("Content", "Tags");

    public static async Task SavePost(Post post)
    {
        string folderName = $"{post.CreatedAt:yyyy-MM-dd}-{post.Slug}";
        string postFolder = Path.Combine(postBasePath, folderName);

        Directory.CreateDirectory(postFolder);
        Directory.CreateDirectory(Path.Combine(postFolder, "assets"));

        string contentPath = Path.Combine(postFolder, "content.md");
        await File.WriteAllTextAsync(contentPath, post.Body);

        string metadataPath = Path.Combine(postFolder, "meta.json");

        var metadata = new
        {
            post.Title,
            post.Description,
            post.Metadata.Tags,
            post.Metadata.Categories,
            post.CustomUrl,
            post.AuthorUsername,
            post.Status,
            post.PublishedAt,
            post.ModifiedAt
        };

        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(metadataPath, json);
    }

    public static bool DeletePost(Post post)
    {
        string folderName = $"{post.PublishedAt:yyyy-MM-dd}-{post.Slug}";
        string postFolder = Path.Combine(postBasePath, folderName);

        if (Directory.Exists(postFolder))
        {
            Directory.Delete(postFolder, recursive: true);
            return true;
        }
        return false;
    }

    public static PostViewDTO? ReadPostFromFile(string slug)
    {
        if (!Directory.Exists(postBasePath))
            return null;

        var postFolder = Directory.GetDirectories(postBasePath).FirstOrDefault(dir => dir.EndsWith(slug));

        if (postFolder == null)
            return null;

        string metadataPath = Path.Combine(postFolder, "meta.json");
        string contentPath = Path.Combine(postFolder, "content.md");
        string assetsPath = Path.Combine(postFolder, "assets");

        if (!File.Exists(metadataPath) || !File.Exists(contentPath))
            return null;

        try
        {
            var metadataJson = File.ReadAllText(metadataPath);
            var contentMd = File.ReadAllText(contentPath);

            var tempDto = JsonSerializer.Deserialize<PostViewDTO>(metadataJson);
            if (tempDto == null)
                return null;

            tempDto.Body = contentMd;

            // Optionally load asset filenames
            if (Directory.Exists(assetsPath))
            {
                tempDto!.Assets = Directory.GetFiles(assetsPath)
                    .Select(Path.GetFileName)
                    .OfType<string>()
                    .ToList();
            }


            return tempDto;
        }
        catch
        {
            return null;
        }
    }

    public static async Task SaveUser(User user)
    {
        string filePath = Path.Combine(userBasePath, user.Username);
        Directory.CreateDirectory(filePath);

        string metadataPath = Path.Combine(filePath, "profile.json");

        var metadata = new
        {
            user.Username,
            user.Email,
            user.Role
        };

        var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(metadataPath, json);
    }

    public static void SaveTagIfNotExists(string slug)
    {
        Directory.CreateDirectory(tagBasePath);
        string path = Path.Combine(tagBasePath, $"{slug}.json");
        if (File.Exists(path)) return;

        var tag = new Tag
        {
            Slug = slug,
            DisplayName = SlugToName(slug)
        };

        var json = JsonSerializer.Serialize(tag, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static void SaveCategoryIfNotExists(string slug)
    {
        Directory.CreateDirectory(categoryBasePath);
        string path = Path.Combine(categoryBasePath, $"{slug}.json");
        if (File.Exists(path)) return;

        var category = new Category
        {
            Slug = slug,
            DisplayName = SlugToName(slug)
        };

        var json = JsonSerializer.Serialize(category, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static async Task<bool> CustomUrlExists(string customUrl)
    {
        if (!Directory.Exists(postBasePath))
            return false;

        var allDirs = Directory.GetDirectories(postBasePath);

        foreach (var dir in allDirs)
        {
            string metaPath = Path.Combine(dir, "meta.json");
            if (!File.Exists(metaPath)) continue;

            try
            {
                using FileStream stream = File.OpenRead(metaPath);
                var dto = await JsonSerializer.DeserializeAsync<PostViewDTO>(stream);
                if (dto?.CustomUrl == customUrl)
                    return true;
            }
            catch
            {
                continue;
            }
        }

        return false;
    }

    public static async Task<Post?> GetPostByCustomUrl(string customUrl)
    {
        if (!Directory.Exists(postBasePath))
            return null;

        var allDirs = Directory.GetDirectories(postBasePath);

        foreach (var dir in allDirs)
        {
            string metaPath = Path.Combine(dir, "meta.json");
            string contentPath = Path.Combine(dir, "content.md");

            if (!File.Exists(metaPath) || !File.Exists(contentPath))
                continue;

            try
            {
                using FileStream stream = File.OpenRead(metaPath);
                var metadata = await JsonSerializer.DeserializeAsync<PostViewDTO>(stream);
                if (metadata == null || metadata.CustomUrl != customUrl)
                    continue;

                var body = await File.ReadAllTextAsync(contentPath);

                var post = new Post
                {
                    Title = metadata.Title,
                    Description = metadata.Description,
                    Slug = Slugify(metadata.Title),
                    Body = body,
                    PublishedAt = metadata.PublishedAt,
                    AuthorUsername = metadata.AuthorUsername,
                    Metadata = metadata.Metadata,
                    CustomUrl = metadata.CustomUrl,
                    Assets = metadata.Assets
                };

                return post;
            }
            catch
            {
                continue; 
            }
        }

        return null;
    }


    private static string SlugToName(string slug)
    {
        return string.Join(' ',
            slug.Split('-')
                .Select(s => char.ToUpper(s[0]) + s.Substring(1))
        );
    }

    private static string Slugify(string title)
    {
        return title.ToLower().Replace(" ", "-");
    }

}

