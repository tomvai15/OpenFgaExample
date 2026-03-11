using OpenFga.Sdk.Model;

namespace OpenFgaExample.Api.Extensions;

public static class TupleKeyExtensions
{
    public static string GetUserId(this TupleKey key)
    {
        var parts = key.User.Split(':');
        return parts[1];
    }
}