using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Queries;

public sealed class GetIftaTaxRateQuery : IQuery<Result<IftaTaxRateDto>>
{
    public Guid Id { get; set; }
}
