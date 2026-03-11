using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using OpenFgaExample.Core.Interfaces;
using OpenFgaExample.Api.Models;

namespace OpenFgaExample.Api.Services;

public class InitialRelationsSeeder(
    OpenFgaClient openFgaClient,
    IProjectRepository projectRepository,
    ILogger<InitialRelationsSeeder> logger)
{
    // use centralized test users from TestUserStore
    private static IEnumerable<KeyValuePair<string, TestUserModel>> TestUsers => TestUserStore.Entries;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("InitialRelationsSeeder: starting seed");
        // gather write tuples
        var writes = new List<ClientTupleKey>();

        // user -> organization (member)
        foreach (var kv in TestUsers)
        {
            var userId = kv.Key;
            var orgId = kv.Value.OrganizationId;
            
            var relation = kv.Value.Role switch
            {
                UserRole.Admin => AccessRelations.Organization.Admin,
                UserRole.Member => AccessRelations.Organization.Member,
                UserRole.Viewer => AccessRelations.Organization.Viewer,
                _ => throw new InvalidOperationException($"Unknown role {kv.Value.Role}")
            };
            writes.Add(new ClientTupleKey
            {
                Object = $"Organization:{orgId}",
                Relation = relation,
                User = $"user:{userId}"
            });
        }

        // project -> organization (belongs_to)
        var projects = (await projectRepository.GetAllAsync()).ToList();
        foreach (var p in projects)
        {
            writes.Add(new ClientTupleKey
            {
                Object = $"Project:{p.Id}",
                Relation = AccessRelations.Project.Organization,
                User = $"Organization:{p.OrganizationId}"
            });

            // make first test user owner of every project (optional)
            writes.Add(new ClientTupleKey
            {
                Object = $"Project:{p.Id}",
                Relation = AccessRelations.Project.Owner,
                User = $"user:user-1"
            });
        }

        var writeReq = new ClientWriteRequest
        {
            Writes = writes
        };

        var options = new ClientWriteOptions
        {
            Conflict = new ConflictOptions()
            {
                OnDuplicateWrites = OnDuplicateWrites.Ignore
            }
        };

        try
        {
            logger.LogInformation("InitialRelationsSeeder: writing {Count} tuples", writes.Count);
            await openFgaClient.Write(writeReq, options, cancellationToken: cancellationToken);
            logger.LogInformation("InitialRelationsSeeder: writes complete");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InitialRelationsSeeder: failed to seed relations");
        }
    }
}