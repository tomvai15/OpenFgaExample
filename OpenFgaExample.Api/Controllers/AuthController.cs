using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenFgaExample.Api.Models;
using OpenFgaExample.Api.Services;

namespace OpenFgaExample.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    // test users are defined in TestUserStore

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("users")]
    public IList<TestUserModel> GetTestUsers()
    {
        return TestUserStore.All.ToList();
    }

    [HttpPost("token/{userId}")]
    public ActionResult<TokenResponse> GenerateToken(string userId)
    {
        if (!TestUserStore.TryGet(userId, out var user)) return NotFound();

        var jwtSection = _config.GetSection("Jwt");
        var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("JWT key not configured");
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");
        var expireMinutes = jwtSection.GetValue<int>("ExpireMinutes", 60);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("Organization", user.OrganizationId),
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