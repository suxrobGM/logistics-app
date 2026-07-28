using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AICopilot.Queries;

[RequiresFeature(TenantFeature.AICopilot)]
public class GetAICopilotConversationByIdQuery : IQuery<Result<AICopilotConversationDto>>, IHaveId
{
    public Guid Id { get; set; }
}
