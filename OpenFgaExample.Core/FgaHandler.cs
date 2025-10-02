using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OpenFgaExample.Core.Interfaces;

namespace OpenFgaExample.Core;

public class FgaHandler(IAuthorizationChecker authorizationChecker,
    ICheckRequestProvider checkRequestProvider,
    ILogger<FgaHandler> logger) : AuthorizationHandler<FgaRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        FgaRequirement requirement)
    {
        var checkRequest = checkRequestProvider.GetCheckRequest(requirement);
        if (checkRequest == null)
        {
            logger.LogInformation("Check request could not be retrieved.");
            return;
        }
        
        var check = await authorizationChecker.IsAuthorizedAsync(checkRequest);

        if (check.IsAllowed)
        {
            logger.LogInformation("Authorization check access granted {CheckRequest}.", checkRequest.ToString());
            context.Succeed(requirement);
            return;
        }
        
        logger.LogInformation("Authorization check access failed {CheckRequest}.", checkRequest.ToString());
        context.Fail();
    }
}