namespace Logistics.Domain.Common;

/// <summary>
/// Aggregate root
/// </summary>
public interface IAggregateRoot
{
    /// <summary>
    /// Primary key
    /// </summary>
    string Id { get; set; }
}