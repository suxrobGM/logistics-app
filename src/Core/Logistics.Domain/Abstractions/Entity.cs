using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Abstractions;

public abstract class Entity : IEntity<string>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [NotMapped] 
    public List<IDomainEvent> DomainEvents { get; } = new();
}