using OpenFgaExample.Core.Models;

namespace OpenFgaExample.Core.Interfaces;

public interface ICheckRequestProvider
{ 
    CheckRequest? GetCheckRequest(FgaRequirement requirement);
}