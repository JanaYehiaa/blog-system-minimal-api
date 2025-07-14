using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;

public class CategoriesModel : PageModel
{
    private readonly PostService _postService;

    public CategoriesModel(PostService postService)
    {
        _postService = postService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SelectedCategory { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<PostPreviewDTO> Posts { get; set; } = new();
    public SelectList CategorySelectList { get; set; } = null!;
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public async Task OnGetAsync()
    {
        var categories = await _postService.GetAllCategories();
        CategorySelectList = new SelectList(categories, SelectedCategory);

        if (!string.IsNullOrEmpty(SelectedCategory))
        {
            var result = await _postService.GetPostsByCategory(SelectedCategory, PageNumber, 5);
            Posts = result.Items;
            TotalPages = (int)Math.Ceiling((double)result.TotalCount / 5);
        }
        else
        {
            Posts.Clear();
            TotalPages = 1;
        }
    }
}
