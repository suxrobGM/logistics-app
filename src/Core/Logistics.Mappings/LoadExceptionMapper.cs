using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class LoadExceptionMapper
{
    public static LoadExceptionDto ToDto(this LoadException entity)
    {
        return new LoadExceptionDto
        {
            Id = entity.Id,
            Type = entity.Type,
            Reason = entity.Reason,
            OccurredAt = entity.OccurredAt,
            ResolvedAt = entity.ResolvedAt,
            ReportedByName = entity.ReportedByName,
            Resolution = entity.Resolution
        };
    }
}
