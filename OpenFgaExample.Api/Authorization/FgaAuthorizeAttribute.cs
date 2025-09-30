using Microsoft.AspNetCore.Authorization;

namespace OpenFgaExample.Api.Authorization;

public class FgaAuthorizeAttribute : AuthorizeAttribute
{
    public FgaAuthorizeAttribute(string relation, string objectType)
    {
        Policy = $"{relation}:{objectType}";
    }
}