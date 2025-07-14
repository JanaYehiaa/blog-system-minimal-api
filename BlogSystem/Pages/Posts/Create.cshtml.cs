using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class CreateModel : PageModel
{
    [BindProperty]
    public string Title { get; set; } = string.Empty;
    [BindProperty]
    public string Description { get; set; } = string.Empty;
    [BindProperty]
    public string Body { get; set; } = string.Empty;
    [BindProperty]
    public string CustomUrl { get; set; } = string.Empty;
    [BindProperty]
    public MetaData Metadata { get; set; } = new();
}
