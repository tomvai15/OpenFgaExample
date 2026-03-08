using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFgaExample.Api.Authorization;
using OpenFgaExample.Core.Interfaces;
using OpenFgaExample.Core.Models;
using OpenFgaExample.Api.Models;

namespace OpenFgaExample.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController(IProjectRepository repo) : ControllerBase
{
    [HttpGet]
    [FgaAuthorize(Access.Organization.CanView)]
    public async Task<IActionResult> GetAll()
    {
        var items = await repo.GetAllAsync();
        var res = items.Select(p => new ProjectResponseModel(p.Id.ToString(), p.Name, p.Description));
        return Ok(res);
    }

    [HttpPost]
    [FgaAuthorize(Access.Organization.CanCreate)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest req)
    {
        var project = new Project(Guid.NewGuid(), req.Name, req.Description);
        var created = await repo.CreateAsync(project);
        return CreatedAtAction(nameof(Get), new { id = created.Id },
            new ProjectResponseModel(created.Id.ToString(), created.Name, created.Description));
    }

    [HttpGet("{id:guid}")]
    [FgaAuthorize(Access.Project.CanView, "id")]
    public async Task<IActionResult> Get(Guid id)
    {
        var project = await repo.GetAsync(id);
        if (project is null) return NotFound();
        return Ok(new ProjectResponseModel(project.Id.ToString(), project.Name, project.Description));
    }

    [HttpPut("{id:guid}")]
    [FgaAuthorize(Access.Project.CanEdit, "id")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest req)
    {
        var updated = await repo.UpdateAsync(id, req.Name, req.Description);
        if (updated is null) return NotFound();
        return Ok(new ProjectResponseModel(updated.Id.ToString(), updated.Name, updated.Description));
    }

    [HttpDelete("{id:guid}")]
    [FgaAuthorize(Access.Project.CanDelete, "id")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await repo.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}