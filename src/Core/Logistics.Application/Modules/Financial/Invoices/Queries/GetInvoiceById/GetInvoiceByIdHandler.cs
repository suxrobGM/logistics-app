using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

internal sealed class GetInvoiceByIdHandler(ITenantUnitOfWork tenantUow)
    : GetTenantEntityByIdHandler<GetInvoiceByIdQuery, Invoice, InvoiceDto>(tenantUow)
{
    protected override InvoiceDto MapToDto(Invoice entity) => entity.ToDto();
}
