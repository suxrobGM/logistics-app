using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.IdentityAccess.CustomerUsers.Commands;

internal sealed class DeleteCustomerUserHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteCustomerUserCommand, CustomerUser>(tenantUow);
