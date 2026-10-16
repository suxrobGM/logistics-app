using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

internal sealed class AccountingReceivablesSyncService(
    ITenantUnitOfWork tenantUow,
    IAccountingMappingService mappingService) : IAccountingReceivablesSyncService
{
    public async Task PushAsync(SyncRun run, CancellationToken ct)
    {
        await PushCustomersAsync(run, ct);
        await mappingService.RetireRemovedAsync<Customer>(run, QboLocalEntityType.Customer,
            target => run.Provider.DeactivateCustomerAsync(target, ct), ct);
        await PushInvoicesAsync(run, ct);
        await PushPaymentsAsync(run, ct);
        await mappingService.RetireRemovedAsync<Payment>(run, QboLocalEntityType.Payment,
            target => run.Provider.VoidPaymentAsync(target, ct), ct);
    }

    private async Task PushCustomersAsync(SyncRun run, CancellationToken ct)
    {
        var mappings = mappingService.Mappings;
        var customers = await tenantUow.Repository<Customer>().Query()
            .Where(mappingService.NotUpToDate<Customer>(QboLocalEntityType.Customer))
            .Where(c => c.Status == CustomerStatus.Active || mappings.Any(m =>
                m.LocalEntityType == QboLocalEntityType.Customer && m.LocalId == c.Id && m.QboId != null))
            .OrderBy(c => c.CreatedAt)
            .Take(SyncRun.BatchSize)
            .ToListAsync(ct);

        await mappingService.LoadAsync(run, QboLocalEntityType.Customer, customers.Select(c => c.Id), ct);

        foreach (var customer in customers)
        {
            if (customer.Status != CustomerStatus.Active)
            {
                await mappingService.RetireAsync(run, QboLocalEntityType.Customer, customer.Id,
                    target => run.Provider.DeactivateCustomerAsync(target, ct), ct);
                continue;
            }

            var (payload, hash) = AccountingPayloads.Customer(customer);
            await mappingService.PushAsync(run, QboLocalEntityType.Customer, customer.Id, hash,
                target => run.Provider.UpsertCustomerAsync(payload, target, ct), ct);
        }
    }

    private async Task PushInvoicesAsync(SyncRun run, CancellationToken ct)
    {
        var mappings = mappingService.Mappings;
        var from = run.SyncFrom;
        var invoices = await tenantUow.Repository<LoadInvoice>().Query()
            .Where(mappingService.NotUpToDate<LoadInvoice>(QboLocalEntityType.Invoice))
            .Where(i => i.CreatedAt >= from && i.Status != InvoiceStatus.Draft
                        && i.Status != InvoiceStatus.PendingApproval && i.Status != InvoiceStatus.Rejected)
            .Where(i => i.Status != InvoiceStatus.Cancelled || mappings.Any(m =>
                m.LocalEntityType == QboLocalEntityType.Invoice && m.LocalId == i.Id && m.QboId != null))
            .OrderBy(i => i.CreatedAt)
            .Take(SyncRun.BatchSize)
            .ToListAsync(ct);

        if (invoices.Count == 0)
        {
            return;
        }

        var invoiceIds = invoices.Select(i => i.Id).ToList();
        await mappingService.LoadAsync(run, QboLocalEntityType.Invoice, invoiceIds, ct);
        await mappingService.LoadAsync(run, QboLocalEntityType.Customer, invoices.Select(i => i.CustomerId), ct);
        var lineItems = (await tenantUow.Repository<InvoiceLineItem>()
                .GetListAsync(li => invoiceIds.Contains(li.InvoiceId), ct))
            .ToLookup(li => li.InvoiceId);

        foreach (var invoice in invoices)
        {
            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                await mappingService.RetireAsync(run, QboLocalEntityType.Invoice, invoice.Id,
                    target => run.Provider.VoidInvoiceAsync(target, ct), ct);
                continue;
            }

            var customerQboId = run.SyncedQboId(QboLocalEntityType.Customer, invoice.CustomerId);
            if (customerQboId is null)
            {
                await mappingService.RecordFailureAsync(run, QboLocalEntityType.Invoice, invoice.Id,
                    "The invoice's customer is not in QuickBooks yet. Fix the customer's sync error first.", ct);
                continue;
            }

            var (payload, hash) = AccountingPayloads.Invoice(
                invoice, lineItems[invoice.Id], customerQboId, run.Config, run.Config.Mappings);
            await mappingService.PushAsync(run, QboLocalEntityType.Invoice, invoice.Id, hash,
                target => run.Provider.UpsertInvoiceAsync(payload, target, ct), ct);
        }
    }

    /// <summary>
    /// A factored invoice's payment is the factor buying it, deposited at the full total to the factoring
    /// clearing account rather than the usual deposit account.
    /// </summary>
    private async Task PushPaymentsAsync(SyncRun run, CancellationToken ct)
    {
        var mappings = mappingService.Mappings;
        var from = run.SyncFrom;

        // Payment's invoice FK is a shadow property, so payments are reached through their load invoice.
        var rows = await tenantUow.Repository<LoadInvoice>().Query()
            .SelectMany(i => i.Payments.Where(p =>
                    p.Status == PaymentStatus.Paid && (p.RecordedAt ?? p.CreatedAt) >= from
                    && !mappings.Any(m => m.LocalEntityType == QboLocalEntityType.Payment && m.LocalId == p.Id
                        && m.SyncStatus != QboSyncStatus.Failed && m.LastSyncedAt >= (p.UpdatedAt ?? p.CreatedAt))),
                (i, p) => new { Payment = p, InvoiceId = i.Id, i.CustomerId })
            .OrderBy(r => r.Payment.CreatedAt)
            .Take(SyncRun.BatchSize)
            .ToListAsync(ct);

        if (rows.Count == 0)
        {
            return;
        }

        var paymentIds = rows.Select(r => r.Payment.Id).ToList();
        await mappingService.LoadAsync(run, QboLocalEntityType.Payment, paymentIds, ct);
        await mappingService.LoadAsync(run, QboLocalEntityType.Invoice, rows.Select(r => r.InvoiceId), ct);
        await mappingService.LoadAsync(run, QboLocalEntityType.Customer, rows.Select(r => r.CustomerId), ct);
        var factoredPaymentIds = (await tenantUow.Repository<FactoringBatchItem>().Query()
                .Where(i => i.PaymentId != null && paymentIds.Contains(i.PaymentId.Value))
                .Select(i => i.PaymentId!.Value)
                .ToListAsync(ct))
            .ToHashSet();

        foreach (var row in rows)
        {
            var invoiceQboId = run.SyncedQboId(QboLocalEntityType.Invoice, row.InvoiceId);
            var customerQboId = run.SyncedQboId(QboLocalEntityType.Customer, row.CustomerId);
            if (invoiceQboId is null || customerQboId is null)
            {
                await mappingService.RecordFailureAsync(run, QboLocalEntityType.Payment, row.Payment.Id,
                    "The payment's invoice is not in QuickBooks yet (or was voided). Fix the invoice's sync error first.", ct);
                continue;
            }

            var factored = factoredPaymentIds.Contains(row.Payment.Id);
            if (factored && run.Config.FactoringClearingAccountId is null)
            {
                await mappingService.RecordFailureAsync(run, QboLocalEntityType.Payment, row.Payment.Id,
                    "This invoice was sold to a factoring company. Choose the factoring clearing account in the QuickBooks settings to sync it.", ct);
                continue;
            }

            var depositAccountId = factored ? run.Config.FactoringClearingAccountId : run.Config.DepositAccountId;
            var (payload, hash) = AccountingPayloads.Payment(row.Payment, customerQboId, invoiceQboId, depositAccountId);
            await mappingService.PushAsync(run, QboLocalEntityType.Payment, row.Payment.Id, hash,
                target => run.Provider.UpsertPaymentAsync(payload, target, ct), ct);
        }
    }
}
