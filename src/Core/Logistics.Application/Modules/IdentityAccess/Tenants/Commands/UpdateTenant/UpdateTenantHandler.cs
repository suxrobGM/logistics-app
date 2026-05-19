using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class UpdateTenantHandler(
    IMasterUnitOfWork masterUow,
    IStripeCustomerService stripeCustomerService) : IAppRequestHandler<UpdateTenantCommand, Result>
{
    public async Task<Result> Handle(UpdateTenantCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.Id, ct);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.Id}'");
        }

        tenant.Name = PropertyUpdater.UpdateIfChanged(req.Name, tenant.Name, s => s.Trim().ToLower());
        tenant.CompanyName = PropertyUpdater.UpdateIfChanged(req.CompanyName, tenant.CompanyName);
        tenant.CompanyAddress = PropertyUpdater.UpdateIfChanged(req.CompanyAddress, tenant.CompanyAddress);
        tenant.ConnectionString = PropertyUpdater.UpdateIfChanged(req.ConnectionString, tenant.ConnectionString);
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
        tenant.Settings = PropertyUpdater.UpdateIfChanged(req.Settings, tenant.Settings);

        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            await stripeCustomerService.UpdateCustomerAsync(tenant);
        }

        masterUow.Repository<Tenant>().Update(tenant);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
