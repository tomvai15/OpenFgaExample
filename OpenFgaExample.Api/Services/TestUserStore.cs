using OpenFgaExample.Api.Models;

namespace OpenFgaExample.Api.Services;

public enum UserRole
{
    Viewer = 1,
    Member = 2,
    Admin = 3,
}

public static class TestUserStore
{
    private static readonly Dictionary<string, TestUserModel> _users = new()
    {
        ["user-1"] = new TestUserModel("user-1", "Alice", UserRole.Admin, "organization-1"),
        ["user-2"] = new TestUserModel("user-2", "Bob", UserRole.Member, "organization-1"),
        ["user-3"] = new TestUserModel("user-3", "Eve", UserRole.Viewer, "organization-1"),
    };

    public static IReadOnlyCollection<TestUserModel> All => _users.Values.ToList();

    public static IEnumerable<KeyValuePair<string, TestUserModel>> Entries => _users;

    public static bool TryGet(string id, out TestUserModel? user) => _users.TryGetValue(id, out user!);
}

