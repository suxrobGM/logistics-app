using System.Text.Json.Serialization;

namespace Logistics.Shared.Models;

public record Result : IResult
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }
    public bool Success => string.IsNullOrEmpty(Error);

    public static Result Succeed() => new();
    public static Result Fail(string error) => new() { Error = error };
}

public record Result<T> : Result
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public virtual T? Data { get; init; }

    public static Result<T> Succeed(T result) => new() { Data = result };
    public new static Result<T> Fail(string error) => new() { Error = error };
}
