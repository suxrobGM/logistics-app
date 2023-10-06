namespace Logistics.Shared;

public interface IResponseResult
{
    bool IsSuccess { get; }
    bool IsError { get; }
    string? Error { get; init; }
}
