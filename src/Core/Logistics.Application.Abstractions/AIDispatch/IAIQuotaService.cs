using Logistics.Shared.Models;

namespace Logistics.Application.Abstractions.AIDispatch;

public interface IAIQuotaService
{
    Task<AIQuotaStatus> GetQuotaStatusAsync(Guid tenantId, CancellationToken ct = default);
}
