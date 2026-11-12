using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class FactoringMapper
{
    public static partial FactoringCompanyDto ToDto(this FactoringCompany entity);

    [MapperIgnoreSource(nameof(FactoringBatch.FactoringCompany))]
    [MapperIgnoreSource(nameof(FactoringBatch.Items))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.FactoringCompanyName))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.InvoiceCount))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.AdvanceTotal))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.FeeTotal))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.ReserveOutstanding))]
    [MapperIgnoreTarget(nameof(FactoringBatchDto.Items))]
    private static partial FactoringBatchDto ToDtoCore(this FactoringBatch entity);

    extension(FactoringBatch entity)
    {
        /// <summary>
        /// <paramref name="items"/> gives the totals; pass <paramref name="itemDtos"/> only on the detail page, where
        /// the items are listed too.
        /// </summary>
        public FactoringBatchDto ToDto(
            string? factoringCompanyName, IReadOnlyCollection<FactoringBatchItem> items,
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
    }

    [MapperIgnoreSource(nameof(FactoringBatchItem.Batch))]
    [MapperIgnoreSource(nameof(FactoringBatchItem.Invoice))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.InvoiceNumber))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.LoadNumber))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.CustomerName))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.EstimatedAdvance))]
    [MapperIgnoreTarget(nameof(FactoringBatchItemDto.EstimatedFee))]
    private static partial FactoringBatchItemDto ToDtoCore(this FactoringBatchItem entity);

    extension(FactoringBatchItem entity)
    {
        public FactoringBatchItemDto ToDto(
            long invoiceNumber, long loadNumber, string? customerName,
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
}
