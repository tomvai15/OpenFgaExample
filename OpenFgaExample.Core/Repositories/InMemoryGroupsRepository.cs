using OpenFgaExample.Core.Interfaces;
using OpenFgaExample.Core.Models;

namespace OpenFgaExample.Core.Repositories;

public class InMemoryGroupRepository : IGroupRepository
{
    private readonly Dictionary<Guid, Group> _store = new();

    public InMemoryGroupRepository()
    {
        // seed with a couple of entries
        var p1 = new Group(Guid.Parse("bae25ca3-9673-47ea-a2ee-42cdc58f3b73"), "G1", "organization-1");
        var p2 = new Group(Guid.Parse("57e35bc6-a3ec-462b-b039-737967cba8ca"), "G2", "organization-1");

        _store[p1.Id] = p1;
        _store[p2.Id] = p2;
    }

    public Task<IEnumerable<Group>> GetAllAsync()
    {
        return Task.FromResult(_store.Values.AsEnumerable());
    }

    public Task<IEnumerable<Group>> GetByIdsAsync(IList<Guid> ids)
    {
        var groups = ids
            .Where(id => _store.ContainsKey(id))
            .Select(id => _store[id])
            .ToList();
        return Task.FromResult(groups.AsEnumerable());
    }

    public Task<Group?> GetAsync(Guid id)
    {
        _store.TryGetValue(id, out var group);
        return Task.FromResult(group);
    }

    public Task<Group> CreateAsync(Group group)
    {
        var id = Guid.NewGuid();
        _store[id] = group with { Id = id };
        return Task.FromResult(_store[id]);
    }

    public Task<Group?> UpdateAsync(Guid id, string? name)
    {
        if (!_store.TryGetValue(id, out var existing)) return Task.FromResult<Group?>(null);
        var updated = new Group(existing.Id, name ?? existing.Name, existing.OrganizationId);
        _store[id] = updated;
        return Task.FromResult<Group?>(updated);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        return Task.FromResult(_store.Remove(id));
    }
}