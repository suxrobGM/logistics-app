using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteSubscriptionCommand : IAppRequest
{
    public Guid Id { get; set; }
}
