using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Mappings;

[Mapper]
public static partial class PrivacyMapper
{
    [MapperIgnoreSource(nameof(DataExportRequest.User))]
    [MapperIgnoreSource(nameof(DataExportRequest.DomainEvents))]
    [MapperIgnoreSource(nameof(DataExportRequest.BlobContainer))]
    [MapperIgnoreSource(nameof(DataExportRequest.BlobName))]
    [MapperIgnoreTarget(nameof(DataExportRequestDto.DownloadUrl))]
    public static partial DataExportRequestDto ToDto(this DataExportRequest entity);

    [MapperIgnoreSource(nameof(DataDeletionRequest.User))]
    [MapperIgnoreSource(nameof(DataDeletionRequest.DomainEvents))]
    [MapperIgnoreTarget(nameof(DataDeletionRequestDto.IsCancellable))]
    public static partial DataDeletionRequestDto ToDto(this DataDeletionRequest entity);

    public static DataDeletionRequestDto ToDto(this DataDeletionRequest entity, DateTime now)
    {
        var dto = entity.ToDto();
        return dto with
        {
            IsCancellable = entity.Status == Domain.Primitives.Enums.DataDeletionStatus.Pending
                            && entity.ScheduledFor > now
        };
    }

    [MapperIgnoreSource(nameof(ConsentRecord.User))]
    [MapperIgnoreSource(nameof(ConsentRecord.DomainEvents))]
    [MapperIgnoreSource(nameof(ConsentRecord.IpAddress))]
    [MapperIgnoreSource(nameof(ConsentRecord.UserAgent))]
    public static partial ConsentRecordDto ToDto(this ConsentRecord entity);
}
