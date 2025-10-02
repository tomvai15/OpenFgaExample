using OpenFgaExample.Core.Models;

namespace OpenFgaExample.Core.Interfaces;

public interface IAuthorizationChecker
{
    Task<CheckResult> IsAuthorizedAsync(CheckRequest request);
}