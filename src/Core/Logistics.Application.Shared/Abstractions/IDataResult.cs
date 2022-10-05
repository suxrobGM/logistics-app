namespace Logistics.Application.Shared.Abstractions;

public interface IDataResult
{
    bool Success { get; }
    string? Error { get; init; }
}