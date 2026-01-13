namespace Logistics.Shared.Models;

/// <summary>
///     Pagination metadata for paginated API responses.
/// </summary>
/// <param name="Page">Current page number, starting from 1.</param>
/// <param name="Limit">Number of items per page.</param>
/// <param name="Total">Total number of items across all pages.</param>
/// <param name="TotalPages">Total number of pages.</param>
public record PaginationMeta(int Page, int Limit, int Total, int TotalPages);

/// <summary>
///     Paginated API response containing items and pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
/// <param name="Items">The items for the current page.</param>
/// <param name="Pagination">Pagination metadata.</param>
public record PagedResponse<T>(IEnumerable<T> Items, PaginationMeta Pagination)
{
    /// <summary>
    ///     Creates a PagedResponse from a PagedResult.
    /// </summary>
    public static PagedResponse<T> FromPagedResult(PagedResult<T> result, int page, int pageSize)
    {
        return new PagedResponse<T>(
            result.Value ?? [],
            new PaginationMeta(page, pageSize, result.TotalItems, result.TotalPages)
        );
    }
}

/// <summary>
///     Standard error response for API errors.
/// </summary>
/// <param name="Error">The error message.</param>
/// <param name="Details">Optional field-level validation errors.</param>
public record ErrorResponse(string Error, Dictionary<string, string[]>? Details = null)
{
    /// <summary>
    ///     Creates an ErrorResponse from a Result.
    /// </summary>
    public static ErrorResponse FromResult(IResult result)
    {
        return new ErrorResponse(result.Error ?? "An error occurred");
    }
}
