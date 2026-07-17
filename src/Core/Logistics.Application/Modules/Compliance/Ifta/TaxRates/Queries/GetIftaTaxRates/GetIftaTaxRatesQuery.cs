using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Queries;

public sealed class GetIftaTaxRatesQuery : SearchableQuery, IQuery<PagedResult<IftaTaxRateDto>>
{
    public int? Year { get; set; }
    public int? Quarter { get; set; }
    public string? CountryCode { get; set; }
}
