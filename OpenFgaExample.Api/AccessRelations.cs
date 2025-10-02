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
        CanView
    }
}