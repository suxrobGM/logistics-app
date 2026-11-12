using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class PrivacyMapper
{
    [MapperIgnoreSource(nameof(DataExportRequest.User))]
    [MapperIgnoreTarget(nameof(DataExportRequestDto.DownloadUrl))]
    public static partial DataExportRequestDto ToDto(this DataExportRequest entity);

    [MapperIgnoreSource(nameof(DataDeletionRequest.User))]
    [MapperIgnoreTarget(nameof(DataDeletionRequestDto.IsCancellable))]
    public static partial DataDeletionRequestDto ToDto(this DataDeletionRequest entity);

    extension(DataDeletionRequest entity)
    {
        public DataDeletionRequestDto ToDto(DateTime now)
        {
            var dto = entity.ToDto();
            return dto with
            {
                IsCancellable = entity.Status == Domain.Primitives.Enums.DataDeletionStatus.Pending
                                && entity.ScheduledFor > now
            };
        }
    }

    [MapperIgnoreSource(nameof(ConsentRecord.User))]
    public static partial ConsentRecordDto ToDto(this ConsentRecord entity);
}
