namespace Logistics.Domain.Events;

public class NewLoadCreatedEvent : IDomainEvent
{
    public NewLoadCreatedEvent(ulong loadRefId)
    {
        LoadRefId = loadRefId;
    }
    
    public ulong LoadRefId { get; }
}