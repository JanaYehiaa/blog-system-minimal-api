using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty] public string Username { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new
        {
            Username,
            Password
        };

var client = _httpClientFactory.CreateClient("BlogAPI");
        var response = await client.PostAsJsonAsync("/api/users/login", dto);

        if (!response.IsSuccessStatusCode)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var result = await response.Content.ReadFromJsonAsync<JwtResponse>();
        if (result == null || string.IsNullOrWhiteSpace(result.Token))
        {
            ErrorMessage = "Failed to retrieve token.";
            return Page();
        }

        Response.Cookies.Append("jwt", result.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return RedirectToPage("/Index");
    }

    public class JwtResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
