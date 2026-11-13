using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

/// <summary>
/// Runs a learning pass now instead of waiting for the nightly job. Returns the resulting policy
/// so the page can render it without a second round trip.
/// </summary>
[RequiresFeature(TenantFeature.AIDispatch)]
public class RegenerateAIDispatchPolicyCommand : ICommand<Result<AIDispatchPolicyDto>>;
