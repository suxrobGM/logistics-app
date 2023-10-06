namespace Logistics.Shared;

public interface IResponseResult
{
    bool IsSuccess { get; }
    string? Error { get; init; }
}
