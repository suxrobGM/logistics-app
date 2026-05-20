using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Inspections.Queries;

internal sealed class GetInspectionPartCatalogHandler
    : IAppRequestHandler<GetInspectionPartCatalogQuery, Result<InspectionPartCatalogDto>>
{
    public Task<Result<InspectionPartCatalogDto>> Handle(GetInspectionPartCatalogQuery req, CancellationToken ct)
    {
        var catalog = CargoInspectionPartCategoryExtensions.GetCatalogFor(req.LoadType);

        var dto = new InspectionPartCatalogDto
        {
            LoadType = req.LoadType,
            Categories = [.. catalog.Select(c => new InspectionPartCategoryDto
            {
                Value = c,
                Display = c.GetDescription()
            })]
        };

        return Task.FromResult(Result<InspectionPartCatalogDto>.Ok(dto));
    }
}
