using OpenFgaExample.Api.Services;

namespace OpenFgaExample.Api.Models;

public record TestUserModel(string Id, string Name, UserRole Role, string OrganizationId);

