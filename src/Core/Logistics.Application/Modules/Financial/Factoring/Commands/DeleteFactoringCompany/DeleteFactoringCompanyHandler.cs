using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

/// <summary>
/// Removes a factoring company that has no schedules on record. Its customers stop being factored.
/// </summary>
internal sealed class DeleteFactoringCompanyHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<DeleteFactoringCompanyCommand, Result>
{
    public async Task<Result> Handle(DeleteFactoringCompanyCommand req, CancellationToken ct)
    {
        var company = await tenantUow.Repository<FactoringCompany>().GetByIdAsync(req.Id, ct);
        if (company is null)
        {
            return Result.Fail($"Could not find a factoring company with ID '{req.Id}'");
        }

        if (await tenantUow.Repository<FactoringBatch>().CountAsync(b => b.FactoringCompanyId == req.Id, ct) > 0)
        {
            return Result.Fail($"{company.Name} has schedules of accounts on record and cannot be deleted.");
        }

        tenantUow.Repository<FactoringCompany>().Delete(company);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
