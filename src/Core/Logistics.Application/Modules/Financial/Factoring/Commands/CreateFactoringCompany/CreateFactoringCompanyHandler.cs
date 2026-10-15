using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Commands;

internal sealed class CreateFactoringCompanyHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<CreateFactoringCompanyCommand, Result<FactoringCompanyDto>>
{
    public async Task<Result<FactoringCompanyDto>> Handle(CreateFactoringCompanyCommand req, CancellationToken ct)
    {
        var company = new FactoringCompany { Name = req.Name, SubmissionEmail = req.SubmissionEmail };
        FactoringCompanyFields.Apply(req, company);

        // The first factor a tenant adds is the default, so the customer form can preselect it.
        if (!company.IsDefault && await tenantUow.Repository<FactoringCompany>().CountAsync(ct: ct) == 0)
        {
            company.IsDefault = true;
        }

        if (company.IsDefault)
        {
            await FactoringCompanyFields.ClearOtherDefaultsAsync(tenantUow, company.Id, ct);
        }

        await tenantUow.Repository<FactoringCompany>().AddAsync(company, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result<FactoringCompanyDto>.Ok(company.ToDto());
    }
}
