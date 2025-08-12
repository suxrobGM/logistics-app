namespace Logistics.Shared.Models;

public interface IResult
{
    bool Success { get; }
    string? Error { get; init; }
}
