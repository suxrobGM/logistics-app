using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class DownloadPublicTrackingDocumentQuery : IAppRequest<Result<DocumentDownloadDto>>
{
    public required Guid TenantId { get; init; }
    public required string Token { get; init; }
    public required Guid DocumentId { get; init; }
}
