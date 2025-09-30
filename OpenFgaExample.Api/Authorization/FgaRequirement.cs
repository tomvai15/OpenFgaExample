using Microsoft.AspNetCore.Authorization;

namespace OpenFgaExample.Api.Authorization;

public class FgaRequirement : IAuthorizationRequirement
{
    public string Relation { get; }
    public string ObjectType { get; }

    public FgaRequirement(string relation, string objectType)
    {
        Relation = relation;
        ObjectType = objectType;
    }
}