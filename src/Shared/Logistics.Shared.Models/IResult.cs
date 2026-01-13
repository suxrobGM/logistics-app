namespace Logistics.Shared.Models;

/// <summary>
///     Minimal contract for an operation result used across the application.
/// </summary>
/// <remarks>
///     A result is considered successful when <see cref="Error" /> is <c>null</c> or empty.
///     Implementations should be immutable and serializable.
/// </remarks>
public interface IResult
{
    /// <summary>
    ///     Indicates whether the operation succeeded (<c>true</c>) or failed (<c>false</c>).
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    ///     Error message describing the failure; <c>null</c> or empty when the operation succeeded.
    /// </summary>
    /// <remarks>
    ///     This property is typically set once (via an init-only setter) by factory methods such as
    ///     <see cref="Result.Fail(string)" /> or <see cref="Result{T}.Fail(string)" />.
    /// </remarks>
    string? Error { get; init; }
}
