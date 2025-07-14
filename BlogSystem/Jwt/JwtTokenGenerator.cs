using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public static class JwtGenerator
{
    public static string GenerateJSONWebToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is custom key for practical aspnetcore sample"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "my blog",
            audience: "https://localhost:5222/",
            claims: new List<Claim>
            {
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("username", user.Username)
            },
            notBefore: DateTime.UtcNow,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}