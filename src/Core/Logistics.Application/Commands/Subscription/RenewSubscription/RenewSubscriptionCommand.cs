using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class RenewSubscriptionCommand : ICommand
{
    public Guid Id { get; set; }
}
