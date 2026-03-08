using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpenFgaExample.Api.Models;

namespace OpenFgaExample.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    private static readonly Dictionary<string, TestUserModel> Users = new Dictionary<string, TestUserModel>
    {
        ["user-1"] = new TestUserModel("user-1", "Alice", "admin"),
        ["user-2"] = new TestUserModel("user-2", "Bob", "viewer"),
        ["user-3"] = new TestUserModel("user-3", "Eve", "editor"),
    };

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("users")]
    public IList<TestUserModel> GetTestUsers()
    {
        return Users.Values.ToList();
    }

    [HttpPost("token/{userId}")]
    public ActionResult<TokenResponse> GenerateToken(string userId)
    {
        if (!Users.TryGetValue(userId, out var user)) return NotFound();

        var jwtSection = _config.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("JWT key not configured");
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");
        var expireMinutes = jwtSection.GetValue<int>("ExpireMinutes", 60);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new TokenResponse(tokenStr));
    }
}