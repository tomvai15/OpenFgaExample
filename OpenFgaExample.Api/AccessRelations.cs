namespace OpenFgaExample.Api;

public abstract class Access
{
    public enum Project
    {
        CanView,
        CanEdit,
        CanDelete
    }

    public enum Organization
    {
        CanCreate,
        CanView,
        CanCreateGroup
    }
    
    public enum Group
    {
        CanView,
        CanEdit,
        CanDelete
    }
}

public static class AccessRelations
{
    public static class Group
    {
        public const string Owner = "Owner";
        public const string Member = "Owner";
        public const string Organization = "Organization";
        public const string Viewer = "Viewer";
        public const string Editor = "Editor";
    }
    
    public static class Project
    {
        public const string Owner = "Owner";
        public const string Organization = "Organization";
        public const string Viewer = "Viewer";
        public const string Editor = "Editor";
    }

    public static class Organization
    {
        public const string Member = "Member";
        public const string Admin = "Admin";
        public const string Viewer = "Viewer";
    }
}