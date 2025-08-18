namespace Logistics.Domain.Core;

/// <summary>
///     Base interface for entities.
/// </summary>
/// <typeparam name="TKey">The type of the entity's identifier.</typeparam>
public interface IEntity<TKey>
{
    public TKey Id { get; set; }
}
