namespace Logistics.Shared;

public interface IResponseResult
{
    bool Success { get; }
    string? Error { get; init; }
}