
namespace Logistics.Application.Modules.Tenancy.Subscriptions.Commands;

public class DeleteSubscriptionPlanCommand : ICommand, IHaveId
{
    public Guid Id { get; set; }
}
