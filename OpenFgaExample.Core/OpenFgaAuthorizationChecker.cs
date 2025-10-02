using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using OpenFgaExample.Core.Interfaces;
using OpenFgaExample.Core.Models;

namespace OpenFgaExample.Core;

public class OpenFgaAuthorizationChecker(OpenFgaClient openFgaClient): IAuthorizationChecker
{
    public async Task<CheckResult> IsAuthorizedAsync(CheckRequest request)
    {
        var check = await openFgaClient.Check(new ClientCheckRequest
        {
            User = request.User,
            Relation = request.Relation,
            Object = request.Object,
        });

        return new CheckResult(check.Allowed ?? false);
    }
}