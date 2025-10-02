using Microsoft.AspNetCore.Mvc;
using OpenFgaExample.Api.Authorization;

namespace OpenFgaExample.Api.Controllers;

[ApiController]
[Route("projects")]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    [FgaAuthorize(Access.Organization.CanView)]
    public IActionResult GetAll()
    {
        return Ok();
    }
    
    [HttpPost]
    [FgaAuthorize(Access.Organization.CanCreate)]
    public IActionResult Create()
    {
        return Ok();
    }
    
    [HttpGet("{id:guid}")]
    [FgaAuthorize(Access.Project.CanView, "id")]
    public IActionResult Get(Guid id)
    {
        return Ok(id);
    }
    
    [HttpPut("{id:guid}")]
    [FgaAuthorize(Access.Project.CanEdit, "id")]
    public IActionResult Update(Guid id)
    {
        return Ok(id);
    }
    
    [HttpDelete("{id:guid}")]
    [FgaAuthorize(Access.Project.CanDelete, "id")]
    public IActionResult Delete(Guid id)
    {
        return Ok(id);
    }
}