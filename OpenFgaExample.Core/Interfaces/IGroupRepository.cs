using OpenFgaExample.Core.Models;

namespace OpenFgaExample.Core.Interfaces;

public interface IGroupRepository
{
    Task<IEnumerable<Group>> GetAllAsync();
    Task<IEnumerable<Group>> GetByIdsAsync(IList<Guid> ids);
    Task<Group?> GetAsync(Guid id);
    Task<Group> CreateAsync(Group project);
    Task<Group?> UpdateAsync(Guid id, string? name);
    Task<bool> DeleteAsync(Guid id);
}

