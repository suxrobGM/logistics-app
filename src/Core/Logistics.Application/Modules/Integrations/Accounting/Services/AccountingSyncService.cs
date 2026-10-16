using System.Linq.Expressions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Application.Abstractions.Features;
using Logistics.Domain.Core;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.Accounting.Services;

internal sealed partial class AccountingSyncService(
    ITenantUnitOfWork tenantUow,
    IFeatureService featureService,
    IAccountingConnectionService connectionService,
    ILogger<AccountingSyncService> logger) : IAccountingSyncService
{
    /// <summary>Records per type per run, so a first sync of a large history spreads over several runs.</summary>
    public const int BatchSize = 500;

    private IQueryable<QboEntityMapping> Mappings => tenantUow.Repository<QboEntityMapping>().Query();

    public async Task SyncAsync(CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        if (!await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.Accounting))
        {
            return;
        }

        var config = await tenantUow.Repository<AccountingProviderConfiguration>()
            .GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline && c.IsActive, ct);
        if (config is null)
        {
            return;
        }

        var provider = await connectionService.GetProviderAsync(config, ct);
        if (provider is null)
        {
            logger.LogInformation("Skipping accounting sync for {TenantName}: QuickBooks needs to be reconnected", tenant.Name);
            return;
        }

        var run = new SyncRun(provider, config, config.Mappings, DateTime.UtcNow);
        try
        {
            await PushCustomersAsync(run, ct);
            await RetireRemovedAsync<Customer>(run, QboLocalEntityType.Customer,
                target => run.Provider.DeactivateCustomerAsync(target, ct), ct);
            await PushInvoicesAsync(run, ct);
            await PushPaymentsAsync(run, ct);
            await RetireRemovedAsync<Payment>(run, QboLocalEntityType.Payment,
                target => run.Provider.VoidPaymentAsync(target, ct), ct);
            await PushFactoringFeesAsync(run, ct);
            await PushExpensesAsync(run, ct);
            await PushPayrollAsync(run, ct);
        }
        catch (AccountingReconnectRequiredException ex)
        {
            logger.LogWarning(ex, "QuickBooks rejected the access token during sync for {TenantName}", tenant.Name);
            await connectionService.RejectAccessTokenAsync(config, ct);
            return;
        }

        config.LastSyncedAt = run.StartedAt;
        await tenantUow.SaveChangesAsync(ct);
    }

    private async Task PushCustomersAsync(SyncRun run, CancellationToken ct)
    {
        var mappings = Mappings;
        var customers = await tenantUow.Repository<Customer>().Query()
            .Where(NotUpToDate<Customer>(QboLocalEntityType.Customer))
            .Where(c => c.Status == CustomerStatus.Active || mappings.Any(m =>
                m.LocalEntityType == QboLocalEntityType.Customer && m.LocalId == c.Id && m.QboId != null))
            .OrderBy(c => c.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        await LoadMappingsAsync(run, QboLocalEntityType.Customer, customers.Select(c => c.Id), ct);

        foreach (var customer in customers)
        {
            if (customer.Status == CustomerStatus.Active)
            {
                var (payload, hash) = AccountingPayloads.Customer(customer);
                await PushAsync(run, QboLocalEntityType.Customer, customer.Id, hash,
                    target => run.Provider.UpsertCustomerAsync(payload, target, ct), ct);
            }
            else
            {
                await PushAsync(run, QboLocalEntityType.Customer, customer.Id, null,
                    target => run.Provider.DeactivateCustomerAsync(target, ct), ct);
            }
        }
    }

    /// <summary>
    /// Customers and payments are hard-deleted, so a pushed record whose row is gone is retired in QuickBooks:
    /// a customer is made inactive, a payment (a factoring charge-back reverses the sale) is voided.
    /// </summary>
    private async Task RetireRemovedAsync<T>(
        SyncRun run, QboLocalEntityType type, Func<QboWriteTarget, Task<QboUpsertResult>> retire, CancellationToken ct)
        where T : class, IEntity<Guid>, ITenantEntity
    {
        var rows = tenantUow.Repository<T>().Query();
        var orphaned = await Mappings
            .Where(m => m.LocalEntityType == type && m.QboId != null
                        && m.SyncStatus != QboSyncStatus.Retired
                        && !rows.Any(r => r.Id == m.LocalId))
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach (var mapping in orphaned)
        {
            run.Mappings.TryAdd((mapping.LocalEntityType, mapping.LocalId), mapping);
            await PushAsync(run, type, mapping.LocalId, null, retire, ct);
        }
    }

    private async Task PushInvoicesAsync(SyncRun run, CancellationToken ct)
    {
        var mappings = Mappings;
        var from = run.SyncFrom;
        var invoices = await tenantUow.Repository<LoadInvoice>().Query()
            .Where(NotUpToDate<LoadInvoice>(QboLocalEntityType.Invoice))
            .Where(i => i.CreatedAt >= from && i.Status != InvoiceStatus.Draft
                        && i.Status != InvoiceStatus.PendingApproval && i.Status != InvoiceStatus.Rejected)
            .Where(i => i.Status != InvoiceStatus.Cancelled || mappings.Any(m =>
                m.LocalEntityType == QboLocalEntityType.Invoice && m.LocalId == i.Id && m.QboId != null))
            .OrderBy(i => i.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (invoices.Count == 0)
        {
            return;
        }

        var invoiceIds = invoices.Select(i => i.Id).ToList();
        await LoadMappingsAsync(run, QboLocalEntityType.Invoice, invoiceIds, ct);
        await LoadMappingsAsync(run, QboLocalEntityType.Customer, invoices.Select(i => i.CustomerId), ct);
        var lineItems = (await tenantUow.Repository<InvoiceLineItem>()
                .GetListAsync(li => invoiceIds.Contains(li.InvoiceId), ct))
            .ToLookup(li => li.InvoiceId);

        foreach (var invoice in invoices)
        {
            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                await PushAsync(run, QboLocalEntityType.Invoice, invoice.Id, null,
                    target => run.Provider.VoidInvoiceAsync(target, ct), ct);
                continue;
            }

            var customerQboId = run.SyncedQboId(QboLocalEntityType.Customer, invoice.CustomerId);
            if (customerQboId is null)
            {
                await RecordFailureAsync(run, QboLocalEntityType.Invoice, invoice.Id,
                    "The invoice's customer is not in QuickBooks yet. Fix the customer's sync error first.", ct);
                continue;
            }

            var (payload, hash) = AccountingPayloads.Invoice(
                invoice, lineItems[invoice.Id], customerQboId, run.Config, run.AccountMappings);
            await PushAsync(run, QboLocalEntityType.Invoice, invoice.Id, hash,
                target => run.Provider.UpsertInvoiceAsync(payload, target, ct), ct);
        }
    }

    private async Task PushPaymentsAsync(SyncRun run, CancellationToken ct)
    {
        var mappings = Mappings;
        var from = run.SyncFrom;

        // Payment's invoice FK is a shadow property, so payments are reached through their load invoice.
        var rows = await tenantUow.Repository<LoadInvoice>().Query()
            .SelectMany(i => i.Payments.Where(p =>
                    p.Status == PaymentStatus.Paid && (p.RecordedAt ?? p.CreatedAt) >= from
                    && !mappings.Any(m => m.LocalEntityType == QboLocalEntityType.Payment && m.LocalId == p.Id
                        && m.SyncStatus != QboSyncStatus.Failed && m.LastSyncedAt >= (p.UpdatedAt ?? p.CreatedAt))),
                (i, p) => new { Payment = p, InvoiceId = i.Id, i.CustomerId })
            .OrderBy(r => r.Payment.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (rows.Count == 0)
        {
            return;
        }

        await LoadMappingsAsync(run, QboLocalEntityType.Payment, rows.Select(r => r.Payment.Id), ct);
        await LoadMappingsAsync(run, QboLocalEntityType.Invoice, rows.Select(r => r.InvoiceId), ct);
        await LoadMappingsAsync(run, QboLocalEntityType.Customer, rows.Select(r => r.CustomerId), ct);
        var factoringSales = await FactoringSalePaymentIdsAsync(rows.Select(r => r.Payment.Id).ToList(), ct);

        foreach (var row in rows)
        {
            var invoiceQboId = run.SyncedQboId(QboLocalEntityType.Invoice, row.InvoiceId);
            var customerQboId = run.SyncedQboId(QboLocalEntityType.Customer, row.CustomerId);
            if (invoiceQboId is null || customerQboId is null)
            {
                await RecordFailureAsync(run, QboLocalEntityType.Payment, row.Payment.Id,
                    "The payment's invoice is not in QuickBooks yet (or was voided). Fix the invoice's sync error first.", ct);
                continue;
            }

            var depositAccountId = run.Config.DepositAccountId;
            if (factoringSales.Contains(row.Payment.Id))
            {
                if (run.Config.FactoringClearingAccountId is null)
                {
                    await RecordFailureAsync(run, QboLocalEntityType.Payment, row.Payment.Id,
                        "This invoice was sold to a factoring company. Choose the factoring clearing account in the QuickBooks settings to sync it.", ct);
                    continue;
                }

                depositAccountId = run.Config.FactoringClearingAccountId;
            }

            var (payload, hash) = AccountingPayloads.Payment(
                row.Payment, customerQboId, invoiceQboId, depositAccountId);
            await PushAsync(run, QboLocalEntityType.Payment, row.Payment.Id, hash,
                target => run.Provider.UpsertPaymentAsync(payload, target, ct), ct);
        }
    }

    private async Task PushExpensesAsync(SyncRun run, CancellationToken ct)
    {
        var config = run.Config;
        if (config.DefaultPaymentAccountId is null || config.DefaultExpenseAccountId is null)
        {
            logger.LogInformation("Skipping expense sync: the QuickBooks payment and expense accounts are not set");
            return;
        }

        var from = run.SyncFrom;
        var expenses = await tenantUow.Repository<Expense>().Query()
            .Where(NotUpToDate<Expense>(QboLocalEntityType.Expense))
            .Where(e => (e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid) && e.ExpenseDate >= from)
            .OrderBy(e => e.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        await LoadMappingsAsync(run, QboLocalEntityType.Expense, expenses.Select(e => e.Id), ct);

        foreach (var expense in expenses)
        {
            var accountId = AccountingPayloads.ExpenseAccountId(expense, config, run.AccountMappings)!;
            var (payload, hash) = AccountingPayloads.Expense(
                expense, config.DefaultPaymentAccountId, config.DefaultPaymentAccountType, accountId);

            await PushAsync(run, QboLocalEntityType.Expense, expense.Id, hash,
                target => run.Provider.UpsertExpenseAsync(payload, target, ct), ct);
        }
    }

    private async Task PushPayrollAsync(SyncRun run, CancellationToken ct)
    {
        var accountId = run.Config.PayrollExpenseAccountId;
        if (accountId is null)
        {
            return;
        }

        var from = run.SyncFrom;
        var payrolls = await tenantUow.Repository<PayrollInvoice>().Query()
            .Where(NotUpToDate<PayrollInvoice>(QboLocalEntityType.PayrollInvoice))
            .Where(p => (p.Status == InvoiceStatus.Approved || p.Status == InvoiceStatus.PartiallyPaid
                         || p.Status == InvoiceStatus.Paid) && p.PeriodEnd >= from)
            .OrderBy(p => p.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (payrolls.Count == 0)
        {
            return;
        }

        var employeeIds = payrolls.Select(p => p.EmployeeId).Distinct().ToList();
        var employees = (await tenantUow.Repository<Employee>().GetListAsync(e => employeeIds.Contains(e.Id), ct))
            .ToDictionary(e => e.Id);
        await LoadMappingsAsync(run, QboLocalEntityType.PayrollInvoice, payrolls.Select(p => p.Id), ct);
        await LoadMappingsAsync(run, QboLocalEntityType.Employee, employeeIds, ct);

        foreach (var employee in employees.Values)
        {
            var (vendor, vendorHash) = AccountingPayloads.Vendor(employee);
            await PushAsync(run, QboLocalEntityType.Employee, employee.Id, vendorHash,
                target => run.Provider.UpsertVendorAsync(vendor, target, ct), ct);
        }

        foreach (var payroll in payrolls)
        {
            if (!employees.TryGetValue(payroll.EmployeeId, out var employee))
            {
                continue;
            }

            var vendorQboId = run.SyncedQboId(QboLocalEntityType.Employee, employee.Id);
            if (vendorQboId is null)
            {
                await RecordFailureAsync(run, QboLocalEntityType.PayrollInvoice, payroll.Id,
                    $"{employee.FirstName} {employee.LastName} could not be added as a QuickBooks vendor. Fix the employee's sync error first.", ct);
                continue;
            }

            var (bill, hash) = AccountingPayloads.Bill(payroll, vendorQboId, accountId);
            await PushAsync(run, QboLocalEntityType.PayrollInvoice, payroll.Id, hash,
                target => run.Provider.UpsertBillAsync(bill, target, ct), ct);
        }
    }

    /// <summary>
    /// Rows with no mapping, a failed push, or a successful push older than their last edit. Everything
    /// else is skipped without being loaded.
    /// </summary>
    private Expression<Func<T, bool>> NotUpToDate<T>(QboLocalEntityType type) where T : AuditableEntity
    {
        var mappings = Mappings;
        return e => !mappings.Any(m => m.LocalEntityType == type && m.LocalId == e.Id
            && m.SyncStatus != QboSyncStatus.Failed && m.LastSyncedAt >= (e.UpdatedAt ?? e.CreatedAt));
    }

    private async Task LoadMappingsAsync(
        SyncRun run, QboLocalEntityType type, IEnumerable<Guid> localIds, CancellationToken ct)
    {
        var ids = localIds.Distinct().Where(id => !run.Mappings.ContainsKey((type, id))).ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var found = await tenantUow.Repository<QboEntityMapping>()
            .GetListAsync(m => m.LocalEntityType == type && ids.Contains(m.LocalId), ct);
        foreach (var mapping in found)
        {
            run.Mappings[(type, mapping.LocalId)] = mapping;
        }
    }

    /// <summary>
    /// Push one record if new or changed and record the outcome on its mapping. A null
    /// <paramref name="hash"/> retires the record (deactivate or void), which only applies once it was pushed.
    /// A failure is stored and does not abort the run; only a rejected token does. Saves after each record
    /// so a crash loses at most the one in flight (and its request id makes the provider return it again).
    /// </summary>
    private async Task PushAsync(
        SyncRun run, QboLocalEntityType type, Guid localId, string? hash,
        Func<QboWriteTarget, Task<QboUpsertResult>> write, CancellationToken ct)
    {
        run.Mappings.TryGetValue((type, localId), out var mapping);
        var status = hash is null ? QboSyncStatus.Retired : QboSyncStatus.Synced;

        if (mapping is not null && mapping.SyncStatus == status && mapping.LastSyncedHash == hash)
        {
            // Touched but unchanged: mark it current so the selection stops loading it (saved at the end of the run).
            mapping.LastSyncedAt = run.StartedAt;
            return;
        }

        if (hash is null && mapping?.QboId is null)
        {
            return;
        }

        mapping ??= await AddMappingAsync(run, type, localId, ct);

        try
        {
            var result = await write(new QboWriteTarget(mapping.QboId, mapping.QboSyncToken,
                AccountingPayloads.RequestId(localId, hash ?? nameof(QboSyncStatus.Retired))));
            mapping.QboId = result.QboId;
            mapping.QboSyncToken = result.SyncToken;
            mapping.LastSyncedHash = hash;
            mapping.LastSyncedAt = run.StartedAt;
            mapping.SyncStatus = status;
            mapping.LastError = null;
        }
        catch (Exception ex) when (ex is not AccountingReconnectRequiredException and not OperationCanceledException)
        {
            Fail(mapping, ex.Message);
            logger.LogWarning(ex, "Failed to push {Type} {LocalId} to QuickBooks", type, localId);
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    /// <summary>A record that cannot be pushed yet, shown to the user like any other sync error.</summary>
    private async Task RecordFailureAsync(
        SyncRun run, QboLocalEntityType type, Guid localId, string message, CancellationToken ct)
    {
        if (!run.Mappings.TryGetValue((type, localId), out var mapping))
        {
            mapping = await AddMappingAsync(run, type, localId, ct);
        }

        Fail(mapping, message);
        await tenantUow.SaveChangesAsync(ct);
    }

    private async Task<QboEntityMapping> AddMappingAsync(
        SyncRun run, QboLocalEntityType type, Guid localId, CancellationToken ct)
    {
        var mapping = new QboEntityMapping { LocalId = localId, LocalEntityType = type };
        await tenantUow.Repository<QboEntityMapping>().AddAsync(mapping, ct);
        run.Mappings[(type, localId)] = mapping;
        return mapping;
    }

    private static void Fail(QboEntityMapping mapping, string message)
    {
        mapping.SyncStatus = QboSyncStatus.Failed;
        mapping.LastError = message.Length > 2000 ? message[..2000] : message;
    }

    private sealed class SyncRun(
        IAccountingProviderService provider,
        AccountingProviderConfiguration config,
        AccountingMappings accountMappings,
        DateTime startedAt)
    {
        public IAccountingProviderService Provider { get; } = provider;
        public AccountingProviderConfiguration Config { get; } = config;
        public AccountingMappings AccountMappings { get; } = accountMappings;
        public DateTime StartedAt { get; } = startedAt;
        public DateTime SyncFrom { get; } = config.SyncFromDate ?? DateTime.UnixEpoch;
        public Dictionary<(QboLocalEntityType, Guid), QboEntityMapping> Mappings { get; } = [];

        public string? SyncedQboId(QboLocalEntityType type, Guid localId) =>
            Mappings.TryGetValue((type, localId), out var m) && m.SyncStatus != QboSyncStatus.Retired
                ? m.QboId
                : null;
    }
}
