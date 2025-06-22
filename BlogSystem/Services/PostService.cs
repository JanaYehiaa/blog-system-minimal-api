    public class PostService
    {
        public Post CreatePost(PostCreateDTO dto, string authorUsername)
        {
            var post = dto.Adapt<Post>();

            post.Id = GeneratePostId();
            post.Slug = Slugify(dto.Title);
            post.AuthorUsername = authorUsername;
            post.CreatedAt = DateTime.UtcNow;
            post.ModifiedAt = null;
            post.Status = PostStatus.Draft;

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

