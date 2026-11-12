using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class ConditionReportMapper
{
    extension(ConditionDefect entity)
    {
        public ConditionDefectDto ToDto()
        {
            return new ConditionDefectDto
            {
                Id = entity.Id,
                PartCategory = entity.PartCategory,
                PartCategoryDisplay = entity.PartCategory.GetDescription(),
                Description = entity.Description,
                Severity = entity.Severity,
                SeverityDisplay = entity.Severity.GetDescription(),
                Zone = entity.Zone,
                ZoneDisplay = entity.Zone?.GetDescription(),
                DamageType = entity.DamageType,
                DamageCode = entity.DamageType?.GetCode(),
                PhotoDocumentId = entity.PhotoDocumentId
            };
        }
    }
}
