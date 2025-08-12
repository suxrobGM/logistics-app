using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class RenewSubscriptionCommand : IAppRequest
{
    public Guid Id { get; set; }
}
