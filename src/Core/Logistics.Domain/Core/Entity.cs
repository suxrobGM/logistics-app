using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Core;

/// <summary>
///     Base entity contains the ID and a list of domain events.
/// </summary>
public abstract class Entity : IEntity<Guid>
{
    [NotMapped] public List<IDomainEvent> DomainEvents { get; } = [];

    public Guid Id { get; set; } = Guid.NewGuid();
}
