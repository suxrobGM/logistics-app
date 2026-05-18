using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Tracking.Queries;

public class DownloadPublicTrackingDocumentQuery : IQuery<Result<DocumentDownloadDto>>
{
    public required Guid TenantId { get; init; }
    public required string Token { get; init; }
    public required Guid DocumentId { get; init; }
}
