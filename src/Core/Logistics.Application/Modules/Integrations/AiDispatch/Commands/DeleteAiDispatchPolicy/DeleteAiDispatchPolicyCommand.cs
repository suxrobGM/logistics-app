using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Commands;

/// <summary>
/// Erases the learned policy and any dispatcher directives. The nightly job re-learns from the
/// surviving decision history unless the policy is also switched off - the UI has to say so.
/// </summary>
[RequiresFeature(TenantFeature.AgenticDispatch)]
public class DeleteAiDispatchPolicyCommand : ICommand;
