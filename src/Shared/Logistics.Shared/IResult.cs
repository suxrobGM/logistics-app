namespace Logistics.Shared;

public interface IResult
{
    bool Success { get; }
    string? Error { get; init; }
}
