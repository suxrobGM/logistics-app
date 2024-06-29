using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetCompanyStatsQuery : IRequest<Result<CompanyStatsDto>>
{
}
