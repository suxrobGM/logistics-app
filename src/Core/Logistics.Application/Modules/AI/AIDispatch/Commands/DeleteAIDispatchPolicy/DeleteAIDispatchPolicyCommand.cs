using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.AI.AIDispatch.Commands;

/// <summary>Deletes the tenant's learned dispatch policy. The nightly learning job rebuilds it from decision history unless the policy is also disabled.</summary>
[RequiresFeature(TenantFeature.AIDispatch)]
public class DeleteAIDispatchPolicyCommand : ICommand;
