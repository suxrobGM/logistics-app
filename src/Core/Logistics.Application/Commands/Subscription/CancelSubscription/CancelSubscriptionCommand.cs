using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class CancelSubscriptionCommand : ICommand
{
    public Guid Id { get; set; }
}
