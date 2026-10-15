using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal sealed class UpdateFactoringCompanyHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<UpdateFactoringCompanyCommand, Result>
{
    public async Task<Result> Handle(UpdateFactoringCompanyCommand req, CancellationToken ct)
    {
        var company = await tenantUow.Repository<FactoringCompany>().GetByIdAsync(req.Id, ct);
        if (company is null)
        {
            return Result.Fail($"Could not find a factoring company with ID '{req.Id}'");
        }

        FactoringCompanyFields.Apply(req, company);
        if (company.IsDefault)
        {
            await FactoringCompanyFields.ClearOtherDefaultsAsync(tenantUow, company.Id, ct);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
