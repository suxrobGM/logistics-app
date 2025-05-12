using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public class NewLoadCreatedEvent : IDomainEvent
{
    public NewLoadCreatedEvent(Guid loadId)
    {
        LoadId = loadId;
    }
    
    public Guid LoadId { get; }
}
