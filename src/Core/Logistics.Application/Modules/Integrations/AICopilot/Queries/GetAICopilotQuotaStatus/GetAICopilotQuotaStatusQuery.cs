using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

/// <summary>The shared AI quota, gated on AICopilot so copilot-only users can read it.</summary>
[RequiresFeature(TenantFeature.AICopilot)]
public sealed class GetAICopilotQuotaStatusQuery : IQuery<Result<AIQuotaStatusDto>>;
