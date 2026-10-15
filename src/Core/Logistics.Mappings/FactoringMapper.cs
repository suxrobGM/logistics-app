using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class FactoringMapper
{
    [MapperIgnoreSource(nameof(FactoringCompany.DomainEvents))]
    [MapperIgnoreSource(nameof(FactoringCompany.CreatedAt))]
    [MapperIgnoreSource(nameof(FactoringCompany.CreatedBy))]
    [MapperIgnoreSource(nameof(FactoringCompany.UpdatedAt))]
    [MapperIgnoreSource(nameof(FactoringCompany.UpdatedBy))]
    public static partial FactoringCompanyDto ToDto(this FactoringCompany entity);

    [MapperIgnoreSource(nameof(FactoringBatch.DomainEvents))]
    [MapperIgnoreSource(nameof(FactoringBatch.FactoringCompany))]
    [MapperIgnoreSource(nameof(FactoringBatch.Items))]
    [MapperIgnoreSource(nameof(FactoringBatch.CreatedAt))]
    [MapperIgnoreSource(nameof(FactoringBatch.CreatedBy))]
    [MapperIgnoreSource(nameof(FactoringBatch.UpdatedAt))]
    [MapperIgnoreSource(nameof(FactoringBatch.UpdatedBy))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.FactoringCompanyName))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.InvoiceCount))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.AdvanceTotal))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.FeeTotal))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.ReserveOutstanding))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.Items))]
    private static partial FactoringBatchDto ToDtoCore(this FactoringBatch entity);

    /// <summary>
    /// The schedule with its totals. Pass the items already loaded for the batch, and their DTOs only for
    /// the detail read.
    /// </summary>
    public static FactoringBatchDto ToDto(
        this FactoringBatch entity, string? factoringCompanyName, IReadOnlyCollection<FactoringBatchItem> items,
        IEnumerable<FactoringBatchItemDto>? itemDtos = null)
    {
        return entity.ToDtoCore() with
        {
            FactoringCompanyName = factoringCompanyName,
            InvoiceCount = items.Count,
            AdvanceTotal = items.Sum(i => i.AdvanceAmount ?? 0m),
            FeeTotal = items.Sum(i => i.FeeAmount ?? 0m),
            ReserveOutstanding = items.Sum(i => i.ReserveOutstanding),
            Items = itemDtos ?? []
        };
    }

    [MapperIgnoreSource(nameof(FactoringBatchItem.DomainEvents))]
    [MapperIgnoreSource(nameof(FactoringBatchItem.Batch))]
    [MapperIgnoreSource(nameof(FactoringBatchItem.BatchId))]
    [MapperIgnoreSource(nameof(FactoringBatchItem.Invoice))]
    [MapperIgnoreSource(nameof(FactoringBatchItem.PaymentId))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.InvoiceNumber))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.LoadNumber))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.CustomerName))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.EstimatedAdvance))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.EstimatedFee))]
    private static partial FactoringBatchItemDto ToDtoCore(this FactoringBatchItem entity);

    public static FactoringBatchItemDto ToDto(
        this FactoringBatchItem entity, long invoiceNumber, long loadNumber, string? customerName,
        FactoringAmounts estimate)
    {
        return entity.ToDtoCore() with
        {
            InvoiceNumber = invoiceNumber,
            LoadNumber = loadNumber,
            CustomerName = customerName,
            EstimatedAdvance = estimate.Advance,
            EstimatedFee = estimate.Fee
        };
    }
}
