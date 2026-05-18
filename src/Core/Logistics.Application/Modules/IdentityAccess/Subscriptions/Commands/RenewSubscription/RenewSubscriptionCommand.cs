using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

public class RenewSubscriptionCommand : ICommand
{
    public Guid Id { get; set; }
}
