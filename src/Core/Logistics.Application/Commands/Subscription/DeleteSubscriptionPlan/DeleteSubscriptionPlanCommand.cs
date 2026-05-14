using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteSubscriptionPlanCommand : ICommand
{
    public Guid Id { get; set; }
}
