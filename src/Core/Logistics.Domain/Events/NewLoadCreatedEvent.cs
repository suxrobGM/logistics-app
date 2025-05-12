using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public class NewLoadCreatedEvent : IDomainEvent
{
    public NewLoadCreatedEvent(string loadId)
    {
        LoadId = loadId;
    }
    
    public string LoadId { get; }
}
