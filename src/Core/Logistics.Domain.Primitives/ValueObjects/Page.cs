namespace Logistics.Domain.Primitives.ValueObjects;

/// <summary>
/// Represents a page in a paginated result set.
/// </summary>
/// <param name="Number">The page number, starting from 1.</param>
/// <param name="Size"> The number of items per page. If 0, it indicates no specific size.</param>
public readonly record struct Page(int Number = 1, int Size = 0);
