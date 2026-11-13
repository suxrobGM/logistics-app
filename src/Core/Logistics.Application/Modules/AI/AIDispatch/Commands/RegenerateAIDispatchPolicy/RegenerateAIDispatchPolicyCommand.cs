using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

/// <summary>Runs the dispatch policy learning pass immediately instead of waiting for the nightly job, and returns the new policy.</summary>
[RequiresFeature(TenantFeature.AIDispatch)]
public class RegenerateAIDispatchPolicyCommand : ICommand<Result<AIDispatchPolicyDto>>;
