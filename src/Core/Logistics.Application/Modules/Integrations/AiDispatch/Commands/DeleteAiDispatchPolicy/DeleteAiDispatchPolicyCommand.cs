using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

/// <summary>
/// Erases everything the agent has learned for this tenant, plus any dispatcher directives.
/// The nightly job will re-learn from the surviving decision history unless the policy is also
/// switched off - the UI has to say so.
/// </summary>
[RequiresFeature(TenantFeature.AgenticDispatch)]
public class DeleteAiDispatchPolicyCommand : ICommand;
