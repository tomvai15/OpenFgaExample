using OpenFgaExample.Core.Models;

namespace OpenFgaExample.Core.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<Project?> GetAsync(Guid id);
    Task<Project> CreateAsync(Project project);
    Task<Project?> UpdateAsync(Guid id, string? name, string? description);
    Task<bool> DeleteAsync(Guid id);
}

