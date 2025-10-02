namespace OpenFgaExample.Core.Models;

public record CheckRequest
{
    public required string User { get; init; }
    public  required string Relation { get; init; }
    public required string Object { get; init; }
}