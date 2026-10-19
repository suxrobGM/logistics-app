using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Factoring.Services;

internal sealed class FactoringBatchDtoBuilder(ITenantUnitOfWork tenantUow) : IFactoringBatchDtoBuilder
{
    public async Task<FactoringBatchDto> BuildAsync(FactoringBatch batch, CancellationToken ct)
    {
        var company = await tenantUow.Repository<FactoringCompany>().GetByIdAsync(batch.FactoringCompanyId, ct);
        var items = await tenantUow.Repository<FactoringBatchItem>().GetListAsync(i => i.BatchId == batch.Id, ct);

        var invoiceIds = items.Select(i => i.InvoiceId).ToList();
        var invoices = await tenantUow.Repository<LoadInvoice>().Query()
            .Where(i => invoiceIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Number, LoadNumber = i.Load.Number, CustomerName = i.Customer.Name })
            .ToDictionaryAsync(i => i.Id, ct);

        var itemDtos = items
            .OrderBy(i => invoices.TryGetValue(i.InvoiceId, out var inv) ? inv.Number : 0)
            .Select(i =>
            {
                invoices.TryGetValue(i.InvoiceId, out var inv);
                var estimate = company?.EstimateAmounts(i.Amount.Amount) ?? new FactoringAmounts(0m, 0m, i.Amount.Amount);
                return i.ToDto(inv?.Number ?? 0, inv?.LoadNumber ?? 0, inv?.CustomerName, estimate);
            })
            .ToList();

        return batch.ToDto(company?.Name, items, itemDtos);
    }
}
