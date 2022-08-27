namespace Logistics.Application.Shared;

public interface IDataResult
{
    bool Success { get; }
    string? Error { get; init; }
}