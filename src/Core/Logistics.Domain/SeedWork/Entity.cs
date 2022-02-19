namespace Logistics.Domain;

public abstract class Entity : IAggregateRoot
{
    public string Id { get; set; } = Generator.NewGuid();
}