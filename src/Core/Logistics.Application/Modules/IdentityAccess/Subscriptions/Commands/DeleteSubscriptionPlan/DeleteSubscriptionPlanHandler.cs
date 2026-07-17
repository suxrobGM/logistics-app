using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;

internal sealed class DeleteSubscriptionPlanHandler(IMasterUnitOfWork masterUow)
    : DeleteMasterEntityHandler<DeleteSubscriptionPlanCommand, SubscriptionPlan>(masterUow);
