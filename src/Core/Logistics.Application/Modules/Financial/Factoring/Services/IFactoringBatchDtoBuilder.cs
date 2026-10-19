using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Factoring.Services;

/// <summary>
/// Builds schedule DTOs with their invoice details in one query per kind of row, instead of walking the
/// lazy navigations per item.
/// </summary>
public interface IFactoringBatchDtoBuilder : IApplicationService
{
    Task<FactoringBatchDto> BuildAsync(FactoringBatch batch, CancellationToken ct);
}
