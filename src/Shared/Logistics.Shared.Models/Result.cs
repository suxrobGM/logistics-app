using System.Text.Json.Serialization;

namespace Logistics.Shared.Models;

/// <summary>
///     Non-generic operation result without a payload.
/// </summary>
/// <remarks>
///     Use <see cref="Result{T}" /> when you need to return data on success.
/// </remarks>
public record Result : IResult
{
    /// <inheritdoc />
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }

    /// <inheritdoc />
    public bool Success => string.IsNullOrEmpty(Error);

    /// <summary>
    ///     Creates a successful result.
    /// </summary>
    public static Result Succeed()
    {
        return new Result();
    }

    /// <summary>
    ///     Creates a failed result with the specified error message.
    /// </summary>
    public static Result Fail(string error)
    {
        return new Result { Error = error };
    }
}

/// <summary>
///     Operation result that carries a payload of type <typeparamref name="T" /> on success.
/// </summary>
/// <typeparam name="T">The type of the data returned when the operation succeeds.</typeparam>
/// <remarks>
///     By convention, <see cref="Data" /> is only populated for successful results
///     (i.e., when <see cref="Result.Success" /> is <c>true</c>).
/// </remarks>
public record Result<T> : Result
{
    /// <summary>
    ///     The data returned by a successful operation; omitted from JSON when <c>null</c>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public virtual T? Data { get; init; }

    /// <summary>
    ///     Creates a successful result with the specified data payload.
    /// </summary>
    public static Result<T> Succeed(T result)
    {
        return new Result<T> { Data = result };
    }

    /// <summary>
    ///     Creates a failed result with the specified error message.
    /// </summary>
    public new static Result<T> Fail(string error)
    {
        return new Result<T> { Error = error };
    }
}
