using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Tax.Queries;

public record GetTenantTaxRatesQuery : IQuery<Result<IReadOnlyList<TenantTaxRateDto>>>;
