using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

/// <summary>
/// Load document information visible to customers in the portal.
/// </summary>
public class PortalDocumentDto
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public required string OriginalFileName { get; set; }
    public DocumentType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid LoadId { get; set; }
    public long LoadNumber { get; set; }
    public required string LoadName { get; set; }
}
