namespace Logistics.Domain.ValueObjects;

/// <summary>
/// Represents a sorting option for queries.
/// </summary>
/// <param name="Property">The property to sort by, can be a nested property using dot notation.</param>
/// <param name="Descending">Indicates whether the sorting should be in descending order.</param>
public readonly record struct Sort(string Property, bool Descending = false);
