using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

public class SignupModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SignupModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty] public string Username { get; set; } = string.Empty;
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string SelectedRole { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new
        {
            Username,
            Email,
            Password,
            Role = Enum.Parse<UserRole>(SelectedRole, ignoreCase: true)
        };


var client = _httpClientFactory.CreateClient("BlogAPI");
        var response = await client.PostAsJsonAsync("/api/users/register", dto);

if (response.IsSuccessStatusCode)
{
return RedirectToPage("/Index");}

try
{
    var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

    if (errorResponse != null)
    {
        if (errorResponse.TryGetValue("error", out var prop) && errorResponse.TryGetValue("errorMessage", out var val))
            ErrorMessage = $"{prop}: {val}";
        else if (errorResponse.TryGetValue("errorMessage", out var msg))
            ErrorMessage = msg;
        else
            ErrorMessage = "Something went wrong.";
    }
    else
    {
        ErrorMessage = "Server returned no data.";
    }
}
catch
{
ErrorMessage = await response.Content.ReadAsStringAsync();
}


return Page();

    }
}
