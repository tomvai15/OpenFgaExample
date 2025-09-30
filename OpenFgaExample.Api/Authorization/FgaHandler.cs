using Microsoft.AspNetCore.Authorization;
using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;

namespace OpenFgaExample.Api.Authorization;

public class FgaHandler(OpenFgaClient fgaClient) : AuthorizationHandler<FgaRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        FgaRequirement requirement)
    {
        var user = "alice";
        if (string.IsNullOrEmpty(user))
            return;

        // Example: extract object ID from route
        var httpContext = (context.Resource as DefaultHttpContext)!;
        var routeValues = httpContext.Request.RouteValues;
        var objectId = "1";
        if (objectId == null) return;

        var check = await fgaClient.Check(new ClientCheckRequest
        {
            User = $"user:{user}",
            Relation = requirement.Relation,
            Object = $"{requirement.ObjectType}:{objectId}"
        });

        if (check.Allowed == true)
        {
            context.Succeed(requirement);
        }
    }
}