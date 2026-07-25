using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Queries;

[RequiresFeature(TenantFeature.AgenticDispatch)]
public sealed class GetAiDispatchPolicyQuery : IQuery<Result<AiDispatchPolicyDto>>;
