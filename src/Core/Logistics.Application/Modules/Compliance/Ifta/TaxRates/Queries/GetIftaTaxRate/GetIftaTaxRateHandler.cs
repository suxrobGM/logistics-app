using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Queries;

internal sealed class GetIftaTaxRateHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetIftaTaxRateQuery, Result<IftaTaxRateDto>>
{
    public async Task<Result<IftaTaxRateDto>> Handle(GetIftaTaxRateQuery req, CancellationToken ct)
    {
        var rate = await masterUow.Repository<IftaTaxRate>().GetByIdAsync(req.Id, ct);

        if (rate is null)
        {
            return Result<IftaTaxRateDto>.Fail($"IFTA tax rate with ID '{req.Id}' not found");
        }

        return Result<IftaTaxRateDto>.Ok(rate.ToDto());
    }
}
