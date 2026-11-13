using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Queries;

/// <summary>The tenant-wide AI budget, served to both the dispatch board and the copilot drawer.</summary>
[RequiresFeature(TenantFeature.AIDispatch)]
public sealed class GetAIQuotaStatusQuery : IQuery<Result<AIQuotaStatusDto>>;
