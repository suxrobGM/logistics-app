using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Tenancy;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

internal sealed class DeleteTenantHandler(
    ITenantDatabaseService tenantDatabase,
    IMasterUnitOfWork masterRepository,
    IStripeCustomerService stripeCustomerService) : IAppRequestHandler<DeleteTenantCommand, Result>
{
    private readonly IMasterUnitOfWork masterUow = masterRepository;
    private readonly IStripeCustomerService stripeCustomerService = stripeCustomerService;
    private readonly ITenantDatabaseService tenantDatabase = tenantDatabase;

    public async Task<Result> Handle(DeleteTenantCommand req, CancellationToken ct)
    {
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.Id, ct);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.Id}'");
        }

        var isDeleted = await tenantDatabase.DeleteDatabaseAsync(tenant.ConnectionString!);

        if (!isDeleted)
        {
            return Result.Fail("Could not delete the tenant's database");
        }

        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            await stripeCustomerService.DeleteCustomerAsync(tenant.StripeCustomerId);
        }

        masterUow.Repository<Tenant>().Delete(tenant);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
