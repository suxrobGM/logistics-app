using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.AI.AIDispatch;

[Mapper]
public static partial class AIDispatchPolicyMapper
{
    // ModelUsed stays out of the DTO - platform observability only.
    public static partial AIDispatchPolicyDto ToDto(this AIDispatchPolicy entity);
}
