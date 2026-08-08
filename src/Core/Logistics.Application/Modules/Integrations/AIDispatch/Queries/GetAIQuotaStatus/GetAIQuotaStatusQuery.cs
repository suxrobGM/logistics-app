using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

/// <summary>The tenant-wide AI budget, served to both the dispatch board and the copilot drawer.</summary>
[RequiresFeature(TenantFeature.AgenticDispatch)]
public sealed class GetAIQuotaStatusQuery : IQuery<Result<AIQuotaStatusDto>>;
