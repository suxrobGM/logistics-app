using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Payments;
using Logistics.Application.Common;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Tenancy.Tenants.Commands;

internal sealed class UpdateTenantHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeCustomerService stripeCustomerService) : IRequestHandler<UpdateTenantCommand, Result>
{
    public async Task<Result> Handle(UpdateTenantCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.Id, ct);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.Id}'", ErrorCodes.NotFound);
        }

        var currencyChanged = req.Settings is not null && req.Settings.Currency != tenant.Settings.Currency;
        if (currencyChanged && await HasFinancialRecordsAsync(tenant.Id, ct))
        {
            return Result.Fail("The currency can't change once the company has loads, invoices or expenses.");
        }

        tenant.CompanyName = PropertyUpdater.UpdateIfChanged(req.CompanyName, tenant.CompanyName);
        tenant.CompanyAddress = PropertyUpdater.UpdateIfChanged(req.CompanyAddress, tenant.CompanyAddress);
        tenant.BillingEmail = PropertyUpdater.UpdateIfChanged(req.BillingEmail, tenant.BillingEmail);
        tenant.DotNumber = PropertyUpdater.UpdateIfChanged(req.DotNumber, tenant.DotNumber);
        tenant.McNumber = PropertyUpdater.UpdateIfChanged(req.McNumber, tenant.McNumber);
        tenant.VatNumber = PropertyUpdater.UpdateIfChanged(req.VatNumber, tenant.VatNumber);
        tenant.EoriNumber = PropertyUpdater.UpdateIfChanged(req.EoriNumber, tenant.EoriNumber);
        tenant.CompanyRegistrationNumber = PropertyUpdater.UpdateIfChanged(
            req.CompanyRegistrationNumber, tenant.CompanyRegistrationNumber);
        tenant.TaxResidencyCountry = PropertyUpdater.UpdateIfChanged(
            req.TaxResidencyCountry, tenant.TaxResidencyCountry);
        tenant.PhoneNumber = PropertyUpdater.UpdateIfChanged(req.PhoneNumber, tenant.PhoneNumber);
        // AI enablement is admin-only; it changes through UpdateTenantAdminSettings.
        tenant.Settings = PropertyUpdater.UpdateIfChanged(
            req.Settings is null ? null : req.Settings with { AIEnabled = tenant.Settings.AIEnabled },
            tenant.Settings);

        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            await stripeCustomerService.UpdateCustomerAsync(tenant);
        }

        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    /// <summary>Amounts carry no currency of their own, so changing it would relabel every one.</summary>
    private async Task<bool> HasFinancialRecordsAsync(Guid tenantId, CancellationToken ct)
    {
        await tenantUow.SetCurrentTenantByIdAsync(tenantId);
        var hasLoads = await tenantUow.Repository<Load>().Query().AnyAsync(ct);
        var hasInvoices = await tenantUow.Repository<Invoice>().Query().AnyAsync(ct);
        var hasExpenses = await tenantUow.Repository<Expense>().Query().AnyAsync(ct);
        return hasLoads || hasInvoices || hasExpenses;
    }
}
