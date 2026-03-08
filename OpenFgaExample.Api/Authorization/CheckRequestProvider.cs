using OpenFgaExample.Core;
using OpenFgaExample.Core.Interfaces;
using OpenFgaExample.Core.Models;

namespace OpenFgaExample.Api.Authorization;

public class CheckRequestProvider(IHttpContextAccessor httpContextAccessor,
    ILogger<CheckRequestProvider> logger,
    IIdentityProvider identityProvider): ICheckRequestProvider
{
    public CheckRequest? GetCheckRequest(FgaRequirement requirement)
    {
        const string rootAccessType = nameof(Access.Organization);
        var identity = identityProvider.GetCurrentUser();
        if (identity is null)
        {
            logger.LogWarning("User is not authenticated");
            return null;
        }
        
       // var userId = httpContextAccessor.HttpContext?.Request.Headers["UserId"].ToString();
       var userId = identity?.Id;
        
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("User {UserId} is required", userId);
            return null;
        }
        
        if (requirement.ObjectType == rootAccessType)
        {
            const string defaultOrg = "defaultOrg";
            return CreateCheckRequest(requirement, userId, defaultOrg);
        }
        
        var objectId = requirement.ObjectIdParamName != null 
            ? httpContextAccessor.HttpContext?.GetRouteValue(requirement.ObjectIdParamName)?.ToString()
            : null;

        if (string.IsNullOrEmpty(objectId))
        {
            logger.LogWarning("{ObjectId} is required", objectId);
            return null;
        }

        if (requirement.ObjectIdParamName is null)
            throw new InvalidOperationException($"{nameof(requirement.ObjectIdParamName)} cannot be null when {nameof(requirement.ObjectType)} is not {rootAccessType}");
        
        return CreateCheckRequest(requirement, userId, objectId);
    }

    private static CheckRequest CreateCheckRequest(FgaRequirement requirement, string userId, string defaultOrg)
    {
        return new CheckRequest
        {
            User = $"user:{userId}",
            Relation = requirement.Relation,
            Object = $"{requirement.ObjectType}:{defaultOrg}"
        };
    }
}