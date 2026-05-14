using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetTenantTaxRatesQuery : IQuery<Result<IReadOnlyList<TenantTaxRateDto>>>;
