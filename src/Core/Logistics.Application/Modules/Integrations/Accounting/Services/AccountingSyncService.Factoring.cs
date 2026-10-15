using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

/// <summary>
/// Factored invoices post through a clearing account: the factor's purchase is the invoice's payment,
/// deposited there at the full total, and the factor's fee is a purchase paid out of it. The advance and
/// the reserve reach the bank as transfers out of the clearing account, which the bookkeeper records.
/// </summary>
internal sealed partial class AccountingSyncService
{
    private async Task<HashSet<Guid>> FactoringSalePaymentIdsAsync(List<Guid> paymentIds, CancellationToken ct)
    {
        var ids = await tenantUow.Repository<FactoringBatchItem>().Query()
            .Where(i => i.PaymentId != null && paymentIds.Contains(i.PaymentId.Value))
            .Select(i => i.PaymentId!.Value)
            .ToListAsync(ct);
        return ids.ToHashSet();
    }

    private async Task PushFactoringFeesAsync(SyncRun run, CancellationToken ct)
    {
        var clearingAccountId = run.Config.FactoringClearingAccountId;
        var feeAccountId = run.Config.FactoringFeeAccountId;
        if (clearingAccountId is null || feeAccountId is null)
        {
            return;
        }

        var from = run.SyncFrom;
        var fees = await tenantUow.Repository<FactoringBatchItem>().Query()
            .Where(NotUpToDate<FactoringBatchItem>(QboLocalEntityType.FactoringFee))
            .Where(i => (i.Status == FactoringStatus.Funded || i.Status == FactoringStatus.ChargedBack)
                        && i.FeeAmount > 0 && i.FundedAt >= from)
            .OrderBy(i => i.FundedAt)
            .Take(BatchSize)
            .Select(i => new { Item = i, BatchNumber = i.Batch.Number, InvoiceNumber = i.Invoice.Number })
            .ToListAsync(ct);

        await LoadMappingsAsync(run, QboLocalEntityType.FactoringFee, fees.Select(f => f.Item.Id), ct);

        foreach (var fee in fees)
        {
            var (payload, hash) = AccountingPayloads.FactoringFee(
                fee.Item, fee.BatchNumber, fee.InvoiceNumber, clearingAccountId, feeAccountId);
            await PushAsync(run, QboLocalEntityType.FactoringFee, fee.Item.Id, hash,
                target => run.Provider.UpsertExpenseAsync(payload, target, ct), ct);
        }
    }
}
