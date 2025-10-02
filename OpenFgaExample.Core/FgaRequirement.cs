using Microsoft.AspNetCore.Authorization;

namespace OpenFgaExample.Core;

public record FgaRequirement(string Relation, string ObjectType, string? ObjectIdParamName)
    : IAuthorizationRequirement;