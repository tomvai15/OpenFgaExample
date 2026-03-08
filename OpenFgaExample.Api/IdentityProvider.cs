using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using OpenFgaExample.Api.Models;

namespace OpenFgaExample.Api;

public interface IIdentityProvider
{
    TestUserModel? GetCurrentUser();
    void SetCurrentUser(TestUserModel? user);
    void SetFromClaims(ClaimsPrincipal? principal);
    void Clear();
}

public class IdentityProvider : IIdentityProvider
{
    // AsyncLocal ensures values flow with async context and are independent of DI scope
    private static readonly AsyncLocal<TestUserModel?> _current = new();

    public TestUserModel? GetCurrentUser() => _current.Value;

    public void SetCurrentUser(TestUserModel? user) => _current.Value = user;

    public void SetFromClaims(ClaimsPrincipal? principal)
    {
        if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
        {
            _current.Value = null;
            return;
        }

        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.Identity.Name;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
        {
            _current.Value = null;
            return;
        }

        _current.Value = new TestUserModel(id, name, role);
    }

    public void Clear() => _current.Value = null;
}