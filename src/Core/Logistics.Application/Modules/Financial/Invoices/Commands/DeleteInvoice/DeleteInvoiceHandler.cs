using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class DeleteInvoiceHandler(ITenantUnitOfWork tenantUow)
    : DeleteTenantEntityHandler<DeleteInvoiceCommand, Invoice>(tenantUow);
