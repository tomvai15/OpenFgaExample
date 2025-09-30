using Microsoft.AspNetCore.Mvc;
using OpenFgaExample.Api.Authorization;
using OpenFgaExample.Api.Models;

namespace OpenFgaExample.Api.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    [FgaAuthorize("member", "group")]
    public IList<ProjectResponseModel> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new ProjectResponseModel("1", "Project"))
            .ToList();
    }
}