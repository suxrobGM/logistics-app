using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Core;

public abstract class Entity : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [NotMapped] 
    public List<IDomainEvent> DomainEvents { get; } = [];
}