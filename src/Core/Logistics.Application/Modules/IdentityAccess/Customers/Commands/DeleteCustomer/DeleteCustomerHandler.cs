using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

internal sealed class DeleteCustomerHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteCustomerCommand, Customer>(tenantUow);
