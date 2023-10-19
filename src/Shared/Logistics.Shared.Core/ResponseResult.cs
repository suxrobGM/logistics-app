using System.Text.Json.Serialization;

namespace Logistics.Shared;

public class ResponseResult : IResponseResult
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }
    public bool IsSuccess => string.IsNullOrEmpty(Error);
    public bool IsError => !IsSuccess;

    public static ResponseResult CreateSuccess() => new();
    public static ResponseResult CreateError(string error) => new() { Error = error };
}

public class ResponseResult<T> : ResponseResult
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; init; }

    public static ResponseResult<T> CreateSuccess(T result) => new() { Data = result };
    public new static ResponseResult<T> CreateError(string error) => new() { Error = error };
}
