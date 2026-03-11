using OpenFgaExample.Core.Interfaces;
using OpenFgaExample.Core.Models;

namespace OpenFgaExample.Core.Repositories;

public class InMemoryProjectRepository : IProjectRepository
{
    private readonly Dictionary<Guid, Project> _store = new();

    public InMemoryProjectRepository()
    {
        // seed with a couple of entries
        var p1 = new Project( Guid.Parse("bae25ca3-9673-47ea-a2ee-42cdc58f3b73"), "Alpha", "Alpha project", "organization-1");
        var p2 = new Project(Guid.Parse("57e35bc6-a3ec-462b-b039-737967cba8ca"), "Beta","Beta project", "organization-1");
        
        _store[p1.Id] = p1;
        _store[p2.Id] = p2;
    }

    public Task<IEnumerable<Project>> GetAllAsync()
    {
        return Task.FromResult(_store.Values.AsEnumerable());
    }

    public Task<IEnumerable<Project>> GetByIdsAsync(IList<Guid> ids)
    {
        var projects = ids
            .Where(id => _store.ContainsKey(id))
            .Select(id => _store[id])
            .ToList();
        return Task.FromResult(projects.AsEnumerable());
    }

    public Task<Project?> GetAsync(Guid id)
    {
        _store.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task<Project> CreateAsync(Project project)
    {
        var id = Guid.NewGuid();
        
        var p = new Project(id, project.Name, project.Description, project.OrganizationId);
        _store[p.Id] = p;
        return Task.FromResult(p);
    }

    public Task<Project?> UpdateAsync(Guid id, string? name, string? description)
    {
        if (!_store.TryGetValue(id, out var existing)) return Task.FromResult<Project?>(null);
        var updated = new Project(existing.Id, name ?? existing.Name, description ?? existing.Description, existing.OrganizationId);
        _store[id] = updated;
        return Task.FromResult<Project?>(updated);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_store.Remove(id));
    }
}
