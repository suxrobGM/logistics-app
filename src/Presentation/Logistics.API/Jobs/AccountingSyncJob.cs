using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Hangfire;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.API.Jobs;

/// <summary>
///     Hangfire job that pushes tenant financial data (customers, invoices, payments, expenses)
///     to their connected accounting provider (QuickBooks Online). Mirrors <see cref="EldSyncJob"/>:
///     one-way, per-tenant, idempotent by content-hash. Runs every 15 minutes.
/// </summary>
public class AccountingSyncJob(
    ILogger<AccountingSyncJob> logger,
    IServiceScopeFactory scopeFactory)
{
    private static readonly TimeSpan TokenRefreshLeeway = TimeSpan.FromMinutes(5);

    public static void ScheduleJobs()
    {
        RecurringJob.AddOrUpdate<AccountingSyncJob>(
            "accounting-sync",
            job => job.SyncAllTenantsAsync(CancellationToken.None),
            Cron.MinuteInterval(15));
    }

    [AutomaticRetry(Attempts = 2)]
    public Task SyncAllTenantsAsync(CancellationToken ct) =>
        TenantJobRunner.ForEachTenantAsync(scopeFactory, logger, "accounting sync", SyncTenantAsync, ct);

    private async Task SyncTenantAsync(IServiceScope scope, Tenant tenant, CancellationToken ct)
    {
        var uow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
        var factory = scope.ServiceProvider.GetRequiredService<IAccountingProviderFactory>();
        uow.SetCurrentTenant(tenant);

        var config = await uow.Repository<AccountingProviderConfiguration>()
            .GetAsync(c => c.ProviderType == AccountingProviderType.QuickBooksOnline && c.IsActive, ct);

        if (config is null || string.IsNullOrEmpty(config.AccessToken))
        {
            return;
        }

        if (!await EnsureValidTokenAsync(uow, factory, config, ct))
        {
            logger.LogWarning("Skipping accounting sync for {TenantName}: token refresh failed", tenant.Name);
            return;
        }

        var provider = factory.GetProvider(config);

        // Load all mappings once; keep an in-memory index so later stages see IDs minted this run.
        var mappings = (await uow.Repository<QboEntityMapping>().GetListAsync(_ => true, ct))
            .ToDictionary(m => (m.LocalEntityType, m.LocalId));

        await PushCustomersAsync(uow, provider, mappings, ct);
        await PushInvoicesAndPaymentsAsync(uow, provider, mappings, ct);
        await PushExpensesAsync(uow, provider, config, mappings, ct);

        config.LastSyncedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
    }

    private async Task<bool> EnsureValidTokenAsync(
        ITenantUnitOfWork uow, IAccountingProviderFactory factory,
        AccountingProviderConfiguration config, CancellationToken ct)
    {
        if (config.TokenExpiresAt is { } expiry && expiry - DateTime.UtcNow > TokenRefreshLeeway)
        {
            return true;
        }

        if (string.IsNullOrEmpty(config.RefreshToken))
        {
            return false;
        }

        try
        {
            var provider = factory.GetProvider(config);
            var refreshed = await provider.RefreshTokenAsync(config.RefreshToken, ct);
            if (refreshed is null)
            {
                return false;
            }

            config.AccessToken = refreshed.AccessToken;
            config.RefreshToken = refreshed.RefreshToken ?? config.RefreshToken;
            config.TokenExpiresAt = refreshed.ExpiresAt;
            await uow.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to refresh QuickBooks token");
            return false;
        }
    }

    private async Task PushCustomersAsync(
        ITenantUnitOfWork uow, IAccountingProviderService provider,
        Dictionary<(QboLocalEntityType, Guid), QboEntityMapping> mappings, CancellationToken ct)
    {
        var customers = await uow.Repository<Customer>().GetListAsync(c => c.Status == CustomerStatus.Active, ct);

        foreach (var customer in customers)
        {
            var payload = new QboCustomerPayload
            {
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                BillingAddress = customer.Address is null ? null : new QboAddressPayload
                {
                    Line1 = customer.Address.Line1,
                    Line2 = customer.Address.Line2,
                    City = customer.Address.City,
                    State = customer.Address.State,
                    ZipCode = customer.Address.ZipCode,
                    Country = customer.Address.Country
                }
            };

            var hash = Hash(customer.Name, customer.Email, customer.Phone, customer.TaxId,
                customer.Address?.Line1, customer.Address?.City, customer.Address?.State, customer.Address?.ZipCode);

            await UpsertAsync(uow, mappings, QboLocalEntityType.Customer, customer.Id, hash,
                (qboId, token) => provider.UpsertCustomerAsync(payload, qboId, token, ct), ct);
        }
    }

    private async Task PushInvoicesAndPaymentsAsync(
        ITenantUnitOfWork uow, IAccountingProviderService provider,
        Dictionary<(QboLocalEntityType, Guid), QboEntityMapping> mappings, CancellationToken ct)
    {
        var invoices = await uow.Repository<LoadInvoice>()
            .GetListAsync(i => i.Status != InvoiceStatus.Draft && i.Status != InvoiceStatus.Cancelled, ct);

        foreach (var invoice in invoices)
        {
            if (!mappings.TryGetValue((QboLocalEntityType.Customer, invoice.CustomerId), out var customerMap)
                || customerMap.QboId is null)
            {
                logger.LogWarning("Skipping invoice {Number}: customer {CustomerId} not synced to QBO yet",
                    invoice.Number, invoice.CustomerId);
                continue;
            }

            var currency = invoice.Total.Currency;
            var payload = new QboInvoicePayload
            {
                QboCustomerId = customerMap.QboId,
                DocNumber = invoice.Number.ToString(CultureInfo.InvariantCulture),
                CurrencyCode = currency,
                DueDate = invoice.DueDate,
                TaxTotal = invoice.TaxTotal.Amount,
                Lines = invoice.LineItems
                    .OrderBy(li => li.Order)
                    .Select(li => new QboInvoiceLinePayload
                    {
                        Description = li.Description,
                        Amount = li.Amount.Amount,
                        Quantity = li.Quantity
                    })
                    .ToList()
            };

            var lineHash = string.Join('|', invoice.LineItems.Select(li => $"{li.Description}:{li.Amount.Amount}:{li.Quantity}"));
            var hash = Hash(invoice.Number.ToString(CultureInfo.InvariantCulture), invoice.Status.ToString(),
                invoice.Total.Amount.ToString(CultureInfo.InvariantCulture), currency, customerMap.QboId, lineHash);

            var invoiceMap = await UpsertAsync(uow, mappings, QboLocalEntityType.Invoice, invoice.Id, hash,
                (qboId, token) => provider.UpsertInvoiceAsync(payload, qboId, token, ct), ct);

            if (invoiceMap?.QboId is not null)
            {
                await PushPaymentsForInvoiceAsync(uow, provider, mappings, invoice, customerMap.QboId, invoiceMap.QboId, ct);
            }
        }
    }

    private async Task PushPaymentsForInvoiceAsync(
        ITenantUnitOfWork uow, IAccountingProviderService provider,
        Dictionary<(QboLocalEntityType, Guid), QboEntityMapping> mappings,
        LoadInvoice invoice, string qboCustomerId, string qboInvoiceId, CancellationToken ct)
    {
        foreach (var payment in invoice.Payments.Where(p => p.Status == PaymentStatus.Paid))
        {
            var payload = new QboPaymentPayload
            {
                QboCustomerId = qboCustomerId,
                QboInvoiceId = qboInvoiceId,
                Amount = payment.Amount.Amount,
                CurrencyCode = payment.Amount.Currency,
                ReferenceNumber = payment.ReferenceNumber ?? payment.StripePaymentIntentId
            };

            var hash = Hash(payment.Amount.Amount.ToString(CultureInfo.InvariantCulture),
                payment.Amount.Currency, payment.Status.ToString(), qboInvoiceId);

            await UpsertAsync(uow, mappings, QboLocalEntityType.Payment, payment.Id, hash,
                (qboId, token) => provider.UpsertPaymentAsync(payload, qboId, token, ct), ct);
        }
    }

    private async Task PushExpensesAsync(
        ITenantUnitOfWork uow, IAccountingProviderService provider, AccountingProviderConfiguration config,
        Dictionary<(QboLocalEntityType, Guid), QboEntityMapping> mappings, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(config.DefaultPaymentAccountId) || string.IsNullOrEmpty(config.DefaultExpenseAccountId))
        {
            logger.LogInformation("Skipping expense sync: default QBO accounts are not configured");
            return;
        }

        var company = await uow.Repository<CompanyExpense>()
            .GetListAsync(e => e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid, ct);
        var truck = await uow.Repository<TruckExpense>()
            .GetListAsync(e => e.Status == ExpenseStatus.Approved || e.Status == ExpenseStatus.Paid, ct);

        foreach (var expense in company.Cast<Expense>().Concat(truck))
        {
            var payload = new QboExpensePayload
            {
                PaymentAccountId = config.DefaultPaymentAccountId,
                ExpenseAccountId = config.DefaultExpenseAccountId,
                Amount = expense.Amount.Amount,
                CurrencyCode = expense.Amount.Currency,
                TxnDate = expense.ExpenseDate,
                Description = string.IsNullOrEmpty(expense.VendorName)
                    ? expense.Notes
                    : $"{expense.VendorName}{(string.IsNullOrEmpty(expense.Notes) ? "" : $" — {expense.Notes}")}"
            };

            var hash = Hash(expense.Number.ToString(CultureInfo.InvariantCulture),
                expense.Amount.Amount.ToString(CultureInfo.InvariantCulture), expense.Amount.Currency,
                expense.Status.ToString(), expense.VendorName);

            await UpsertAsync(uow, mappings, QboLocalEntityType.Expense, expense.Id, hash,
                (qboId, token) => provider.UpsertExpenseAsync(payload, qboId, token, ct), ct);
        }
    }

    /// <summary>
    ///     Push one entity if new or changed. Looks up the mapping by content-hash, skips when
    ///     unchanged and previously synced, otherwise upserts and records the result. Failures are
    ///     recorded on the mapping and do not abort the run. Returns the (updated) mapping.
    /// </summary>
    private async Task<QboEntityMapping?> UpsertAsync(
        ITenantUnitOfWork uow, Dictionary<(QboLocalEntityType, Guid), QboEntityMapping> mappings,
        QboLocalEntityType type, Guid localId, string hash,
        Func<string?, string?, Task<QboUpsertResult>> upsert, CancellationToken ct)
    {
        mappings.TryGetValue((type, localId), out var mapping);

        if (mapping is { SyncStatus: QboSyncStatus.Synced } && mapping.LastSyncedHash == hash)
        {
            return mapping;
        }

        if (mapping is null)
        {
            mapping = new QboEntityMapping { LocalId = localId, LocalEntityType = type };
            await uow.Repository<QboEntityMapping>().AddAsync(mapping, ct);
            mappings[(type, localId)] = mapping;
        }

        try
        {
            var result = await upsert(mapping.QboId, mapping.QboSyncToken);
            mapping.QboId = result.QboId;
            mapping.QboSyncToken = result.SyncToken;
            mapping.LastSyncedHash = hash;
            mapping.LastSyncedAt = DateTime.UtcNow;
            mapping.SyncStatus = QboSyncStatus.Synced;
            mapping.LastError = null;
        }
        catch (Exception ex)
        {
            mapping.SyncStatus = QboSyncStatus.Failed;
            mapping.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            logger.LogError(ex, "Failed to push {Type} {LocalId} to QBO", type, localId);
        }

        await uow.SaveChangesAsync(ct);
        return mapping;
    }

    private static string Hash(params string?[] parts)
    {
        var joined = string.Join('|', parts.Select(p => p ?? string.Empty));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(joined));
        return Convert.ToHexStringLower(bytes);
    }
}
