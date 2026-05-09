using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Represents a GDPR Article 15 / 20 data subject access &amp; portability request.
/// Stored in the master DB because users span tenants.
/// </summary>
public class DataExportRequest : Entity, IMasterEntity
{
    public required Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DataExportStatus Status { get; set; } = DataExportStatus.Pending;

    /// <summary>
    /// Blob storage container holding the generated ZIP, set when Status becomes Ready.
    /// </summary>
    public string? BlobContainer { get; set; }

    /// <summary>
    /// Blob path within the container, set when Status becomes Ready.
    /// </summary>
    public string? BlobName { get; set; }

    /// <summary>
    /// Tenant context used for the upload — needed at download time so the signed-URL
    /// generator hits the same tenant-scoped container the job wrote to.
    /// </summary>
    public Guid? BlobTenantId { get; set; }

    /// <summary>
    /// When the underlying blob is deleted by the expiry job.
    /// Signed download URLs are short-lived (~1h) and regenerated on demand
    /// from the portal while the blob still exists.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Populated when Status is Failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
