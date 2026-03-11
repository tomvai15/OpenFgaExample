using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenFgaExample.Api.Authorization;
using OpenFgaExample.Core.Interfaces;
using OpenFgaExample.Core.Models;
using OpenFgaExample.Api.Models;
using OpenFga.Sdk.Client.Model;
using OpenFga.Sdk.Client;
using OpenFgaExample.Api.Extensions;
using OpenFgaExample.Api.Services;

namespace OpenFgaExample.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController(
    IProjectRepository repo,
    OpenFgaClient openFgaClient,
    IIdentityProvider identityProvider) : ControllerBase
{
    [HttpGet]
    [FgaAuthorize(Access.Organization.CanView)]
    public async Task<IActionResult> GetAll()
    {
        var identity = identityProvider.GetCurrentUser();
        if (identity is null) return Unauthorized();

        var userId = identity.Id;

        var relations = await openFgaClient.ListObjects(new ClientListObjectsRequest
        {
            User = $"user:{userId}",
            Relation = Access.Project.CanView.ToString(),
            Type = nameof(Access.Project)
        });

        var projectIds = relations.Objects
            .Where(o => o.StartsWith("Project:"))
            .Select(o => o.Split(':')[1])
            .Select(idStr => Guid.TryParse(idStr, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToList();

        var items = await repo.GetByIdsAsync(projectIds);

        return Ok(items);
    }

    [HttpPost]
    [FgaAuthorize(Access.Organization.CanCreate)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest req)
    {
        var identity = identityProvider.GetCurrentUser();

        var project = new Project(Guid.NewGuid(), req.Name, req.Description, identity!.OrganizationId);
        var created = await repo.CreateAsync(project);

        await openFgaClient.Write(new ClientWriteRequest
        {
            Writes = new List<ClientTupleKey>
            {
                new ClientTupleKey
                {
                    Object = $"Project:{created.Id}",
                    Relation = AccessRelations.Project.Owner,
                    User = $"user:{identityProvider.GetCurrentUser().Id}"
                }
            }
        });

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

    
   //  public record ProjectRelationsResponse(IList<ProjectRelationsResponseItem> Relations);
   //  public record ProjectRelationsResponseItem(string Id, string DisplayName, string RelationType);
    
    [HttpGet("{id:guid}/relations")]
    [FgaAuthorize(Access.Project.CanEdit, "id")]
    public async Task<ActionResult<ProjectRelationsResponse>> GetProjectRelations(Guid id)
    {
        var responseItems = new List<ProjectRelationsResponseItem>();

        var project = await repo.GetAsync(id);
        if (project is null) return NotFound();

        var validRelations = new List<string>
        {
            AccessRelations.Project.Editor,
            AccessRelations.Project.Viewer,
        };
        foreach (var relation in validRelations)
        {
            var response = await openFgaClient.Read(new ClientReadRequest
            {
                Object = $"Project:{id.ToString()}",
                Relation = relation
            });

            foreach (var tuple in response.Tuples)
            {
                responseItems.Add(new ProjectRelationsResponseItem(Id: tuple.Key.GetUserId(), string.Empty, relation));
            }
        }

        var users = TestUserStore.All.ToList();

        var resultItems = responseItems.Select(responseItem =>
        {
            var user = users.FirstOrDefault(x => x.Id == responseItem.Id);
            return responseItem with
            {
                DisplayName = user?.Name ?? "Unknown"
            };
        }).ToList();

        return Ok(new ProjectRelationsResponse(resultItems));
    }


    // public record UpdateProjectRelationsRequest(string Relation, string Id);

    [HttpPost("{id:guid}/relations")]
    [FgaAuthorize(Access.Project.CanEdit, "id")]
    public async Task<IActionResult> ProjectRelations(Guid id, [FromBody] UpdateProjectRelationsRequest req)
    {
        var project = await repo.GetAsync(id);
        if (project is null) return NotFound();

        var validRelations = new List<string>
        {
            AccessRelations.Project.Editor,
            AccessRelations.Project.Viewer,
        };

        if (!validRelations.Contains(req.Relation))
            return BadRequest($"Invalid relation {req.Relation}, valid relations: {string.Join(", ", validRelations)}");

        await openFgaClient.Write(new ClientWriteRequest
        {
            Writes = new List<ClientTupleKey>
            {
                new ClientTupleKey
                {
                    Object = $"Project:{id.ToString()}",
                    Relation = req.Relation,
                    User = $"user:{req.Id}"
                }
            }
        });


        return Ok();
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