using Microsoft.AspNetCore.Mvc;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Client;
using OpenFgaExample.Api.Models;

namespace OpenFgaExample.Api.Controllers;

[ApiController]
[Route("api/openfga")]
public class OpenFgaController(OpenFgaClient openFgaClient, IIdentityProvider identityProvider)
    : ControllerBase
{
    [HttpPost("check-relation")]
    public async Task<IActionResult> CheckRelation([FromBody] CheckRelationRequest req)
    {
        var identity = identityProvider.GetCurrentUser();
        if (identity is null) return Unauthorized();

        var userId = identity.Id;
        
        const string rootAccessType = nameof(Access.Organization);
        
        var objectStr = req.ResourceType == rootAccessType 
            ? $"{req.ResourceType}:{identity.OrganizationId}"
            : $"{req.ResourceType}:{req.ResourceId}";

        var checkReq = new ClientCheckRequest
        {
            User = $"user:{userId}",
            Relation = req.Relation,
            Object = objectStr,
        };

        var checkRes = await openFgaClient.Check(checkReq);
        return Ok(new CheckRelationResponse(checkRes.Allowed ?? false));
    }
}

