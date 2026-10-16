using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Documents.Services;

/// <summary>
///     Opens a document's file for download once the caller has decided the requester may see it.
/// </summary>
public interface IDocumentDownloadService : IApplicationService
{
    Task<Result<DocumentDownloadDto>> OpenAsync(Document document, CancellationToken ct = default);
}
