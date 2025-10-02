using Microsoft.AspNetCore.Authorization;

namespace OpenFgaExample.Api.Authorization;

public class FgaAuthorizeAttribute : AuthorizeAttribute
{
    public FgaAuthorizeAttribute(string relation, string objectType)
    {
        Policy = $"{relation}:{objectType}";
    }
    
    public FgaAuthorizeAttribute(object accessPoint, string idParamName)
    {
        var relation = accessPoint.ToString();
        var objectType = accessPoint.GetType().Name;
        Policy = $"{relation}:{objectType}:{idParamName}";
    }
    
    public FgaAuthorizeAttribute(Access.Organization accessRelation)
    {
        var relation = accessRelation.ToString();
        const string objectType = nameof(Access.Organization);
        Policy = $"{relation}:{objectType}";
    }
}