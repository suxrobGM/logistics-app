using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class ConditionReportMapper
{
    public static ConditionDefectDto ToDto(this ConditionDefect entity)
    {
        return new ConditionDefectDto
        {
            Id = entity.Id,
            PartCategory = entity.PartCategory,
            PartCategoryDisplay = entity.PartCategory.GetDescription(),
            Description = entity.Description,
            Severity = entity.Severity,
            SeverityDisplay = entity.Severity.GetDescription()
        };
    }
}
