using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const int PageSize = 5;  

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<PostPreviewDTO> Posts { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public async Task OnGetAsync(int page = 1)
    {
        PageNumber = page;

        var httpClient = _httpClientFactory.CreateClient("BlogAPI");

        var url = $"/posts?page={PageNumber}&pageSize={PageSize}";

        if (!string.IsNullOrWhiteSpace(SearchQuery))
            url += $"&search={Uri.EscapeDataString(SearchQuery)}";

        var response = await httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = await JsonSerializer.DeserializeAsync<PagedResult<PostPreviewDTO>>(
                await response.Content.ReadAsStreamAsync(), jsonOptions);

            if (result != null)
            {
                Posts = result.Items;
                TotalPages = (int)Math.Ceiling((double)result.TotalCount / PageSize);
            }
        }
    }
}
