namespace OpenFgaExample.Api.Models;

public record ProjectRelationsResponse(IList<ProjectRelationsResponseItem> Relations);

public record ProjectRelationsResponseItem(string Id, string DisplayName, string RelationType);