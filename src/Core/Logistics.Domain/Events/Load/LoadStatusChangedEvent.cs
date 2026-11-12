using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Events;

public record LoadStatusChangedEvent(Guid LoadId, LoadStatus OldStatus, LoadStatus NewStatus) : IDomainEvent;
