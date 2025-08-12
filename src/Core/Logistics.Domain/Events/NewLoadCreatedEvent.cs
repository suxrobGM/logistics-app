using Logistics.Domain.Core;

namespace Logistics.Domain.Events;

public record NewLoadCreatedEvent(Guid LoadId) : IDomainEvent;
