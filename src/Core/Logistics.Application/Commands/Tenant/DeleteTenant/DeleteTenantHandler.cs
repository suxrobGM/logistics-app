using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteTenantHandler : IAppRequestHandler<DeleteTenantCommand, Result>
{
    private readonly IMasterUnitOfWork _masterRepository;
    private readonly IStripeService _stripeService;
    private readonly ITenantDatabaseService _tenantDatabase;

    public DeleteTenantHandler(
        ITenantDatabaseService tenantDatabase,
        IMasterUnitOfWork masterRepository,
        IStripeService stripeService)
    {
        _tenantDatabase = tenantDatabase;
        _masterRepository = masterRepository;
        _stripeService = stripeService;
    }

    public async Task<Result> Handle(DeleteTenantCommand req, CancellationToken ct)
    {
        var tenant = await _masterRepository.Repository<Tenant>().GetByIdAsync(req.Id);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.Id}'");
        }

        var isDeleted = await _tenantDatabase.DeleteDatabaseAsync(tenant.ConnectionString!);

        if (!isDeleted)
        {
            return Result.Fail("Could not delete the tenant's database");
        }

        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            await _stripeService.DeleteCustomerAsync(tenant.StripeCustomerId);
        }

        _masterRepository.Repository<Tenant>().Delete(tenant);
        await _masterRepository.SaveChangesAsync();
        return Result.Ok();
    }
}
