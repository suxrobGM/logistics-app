using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CancelSubscriptionCommand : IAppRequest
{
    public Guid Id { get; set; }
}
