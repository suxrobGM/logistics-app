namespace Logistics.Domain.Common;

public abstract class Entity : IAggregateRoot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}