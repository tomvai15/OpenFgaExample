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
[Route("api/groups")]
public class GroupController(
    IGroupRepository repo,
    OpenFgaClient openFgaClient,
    IIdentityProvider identityProvider) : ControllerBase
{
    [HttpGet]
    [FgaAuthorize(Access.Organization.CanView)]
    public async Task<IActionResult> GetAll()
    {
        var items = await repo.GetAllAsync();
        return Ok(items);
    }

    [HttpPost]
    [FgaAuthorize(Access.Organization.CanCreateGroup)]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest req)
    {
        var identity = identityProvider.GetCurrentUser()!;
        var created = await repo.CreateAsync(new Group(Guid.Empty, req.Name, identity.OrganizationId));

        await openFgaClient.Write(new ClientWriteRequest
        {
            Writes = new List<ClientTupleKey>
            {
                new ClientTupleKey
                {
                    Object = $"Group:{created.Id}",
                    Relation = AccessRelations.Group.Owner,
                    User = $"user:{identity.Id}"
                }
            }
        });

        return CreatedAtAction(nameof(Get), new { id = created.Id },
            new GroupResponseModel(created.Id.ToString(), created.Name));
    }

    [HttpGet("{id:guid}")]
    [FgaAuthorize(Access.Group.CanView, "id")]
    public async Task<IActionResult> Get(Guid id)
    {
        var project = await repo.GetAsync(id);
        if (project is null) return NotFound();
        return Ok(new GroupResponseModel(project.Id.ToString(), project.Name));
    }

    [HttpPut("{id:guid}")]
    [FgaAuthorize(Access.Group.CanEdit, "id")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest req)
    {
        var updated = await repo.UpdateAsync(id, req.Name);
        if (updated is null) return NotFound();
        return Ok(new GroupResponseModel(updated.Id.ToString(), updated.Name));
    }


    //  public record ProjectRelationsResponse(IList<ProjectRelationsResponseItem> Relations);
    //  public record ProjectRelationsResponseItem(string Id, string DisplayName, string RelationType);

    [HttpGet("{id:guid}/relations")]
    [FgaAuthorize(Access.Group.CanEdit, "id")]
    public async Task<ActionResult<GroupRelationsResponse>> GetProjectRelations(Guid id)
    {
        var responseItems = new List<RelationsResponseItem>();

        var project = await repo.GetAsync(id);
        if (project is null) return NotFound();

        var validRelations = new List<string>
        {
            AccessRelations.Project.Editor,
        };
        foreach (var relation in validRelations)
        {
            var response = await openFgaClient.Read(new ClientReadRequest
            {
                Object = $"Group:{id.ToString()}",
                Relation = relation
            });

            foreach (var tuple in response.Tuples)
            {
                responseItems.Add(new RelationsResponseItem(Id: tuple.Key.GetUserId(), string.Empty, relation));
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

        return Ok(new GroupRelationsResponse(resultItems));
    }


    // public record UpdateProjectRelationsRequest(string Relation, string Id);

    [HttpPost("{id:guid}/relations")]
    [FgaAuthorize(Access.Project.CanEdit, "id")]
    public async Task<IActionResult> GroupRelations(Guid id, [FromBody] UpdateProjectRelationsRequest req)
    {
        var group = await repo.GetAsync(id);
        if (group is null) return NotFound();

        var validRelations = new List<string>
        {
            AccessRelations.Group.Member,
        };

        if (!validRelations.Contains(req.Relation))
            return BadRequest($"Invalid relation {req.Relation}, valid relations: {string.Join(", ", validRelations)}");

        await openFgaClient.Write(new ClientWriteRequest
        {
            Writes = new List<ClientTupleKey>
            {
                new ClientTupleKey
                {
                    Object = $"Group:{id.ToString()}",
                    Relation = req.Relation,
                    User = $"user:{req.Id}"
                }
            }
        });


        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [FgaAuthorize(Access.Group.CanDelete, "id")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await repo.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}