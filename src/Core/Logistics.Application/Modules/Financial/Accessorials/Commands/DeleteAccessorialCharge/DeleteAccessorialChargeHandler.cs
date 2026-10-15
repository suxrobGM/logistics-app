using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Financial.Accessorials.Commands;

internal sealed class DeleteAccessorialChargeHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteAccessorialChargeCommand, AccessorialCharge>(tenantUow);
