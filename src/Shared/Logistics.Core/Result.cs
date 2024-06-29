using System.Text.Json.Serialization;

namespace Logistics.Shared;

public class Result : IResult
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }
    public bool Success => string.IsNullOrEmpty(Error);

    public static Result Succeed() => new();
    public static Result Fail(string error) => new() { Error = error };
}

public class Result<T> : Result
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; init; }

    public static Result<T> Succeed(T result) => new() { Data = result };
    public new static Result<T> Fail(string error) => new() { Error = error };
}
