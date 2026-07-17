using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class DeletePaymentHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeletePaymentCommand, Payment>(tenantUow);
