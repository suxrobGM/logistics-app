using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AICopilot.Queries;

/// <summary>Returns the tenant's weekly AI quota usage. Gated on AICopilot rather than AIDispatch so copilot-only tenants can show the quota widget.</summary>
[RequiresFeature(TenantFeature.AICopilot)]
public sealed class GetAICopilotQuotaStatusQuery : IQuery<Result<AIQuotaStatusDto>>;
