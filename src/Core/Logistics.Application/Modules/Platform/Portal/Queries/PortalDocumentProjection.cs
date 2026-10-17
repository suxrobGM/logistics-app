using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Portal.Queries;

internal static class PortalDocumentProjection
{
    /// <summary>
    /// Projects in SQL so the load label comes in the same query and blob locations never leave the server.
    /// </summary>
    public static IQueryable<PortalDocumentDto> ToPortalDocuments(this IQueryable<LoadDocument> documents) =>
        documents
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new PortalDocumentDto
            {
                Id = d.Id,
                FileName = d.FileName,
                OriginalFileName = d.OriginalFileName,
                Type = d.Type,
                CreatedAt = d.CreatedAt,
                LoadId = d.LoadId,
                LoadNumber = d.Load.Number,
                LoadName = d.Load.Name
            });
}
