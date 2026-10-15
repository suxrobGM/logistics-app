using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Factoring.Queries;

internal sealed class GetFactoringCompaniesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetFactoringCompaniesQuery, Result<List<FactoringCompanyDto>>>
{
    public async Task<Result<List<FactoringCompanyDto>>> Handle(GetFactoringCompaniesQuery req, CancellationToken ct)
    {
        var companies = await tenantUow.Repository<FactoringCompany>().Query()
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

        return Result<List<FactoringCompanyDto>>.Ok(companies.Select(f => f.ToDto()).ToList());
    }
}
