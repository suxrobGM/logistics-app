namespace Logistics.Domain.Primitives;

/// <summary>
/// System-wide defaults for GDPR data subject rights and retention.
/// </summary>
public static class PrivacyDefaults
{
    /// <summary>
    /// How long the generated export ZIP is retained in blob storage before
    /// <c>DataExportExpiryJob</c> deletes it and marks the request Expired.
    /// </summary>
    public static readonly TimeSpan ExportBlobRetention = TimeSpan.FromDays(7);

    /// <summary>
    /// Lifetime of an individual signed download URL. Regenerated on demand from
    /// the portal — emails link back to the portal, never to a long-lived URL.
    /// </summary>
    public static readonly TimeSpan ExportSignedUrlLifetime = TimeSpan.FromHours(1);

    /// <summary>
    /// Grace period between a deletion request and irreversible anonymization.
    /// </summary>
    public static readonly TimeSpan DeletionGracePeriod = TimeSpan.FromDays(30);

    /// <summary>
    /// Read notifications older than this are purged by the retention job.
    /// </summary>
    public static readonly TimeSpan NotificationRetention = TimeSpan.FromDays(365);

    /// <summary>
    /// Dispatch session transcripts older than this are purged by the retention job.
    /// </summary>
    public static readonly TimeSpan AiDispatchSessionRetention = TimeSpan.FromDays(730);

    /// <summary>
    /// Maximum data export requests a single user can submit per 24h.
    /// </summary>
    public const int ExportRateLimitPerDay = 1;

    /// <summary>
    /// Blob storage container holding generated export ZIPs. Lower-case with no
    /// underscores so it satisfies Azure Blob Storage container-name rules.
    /// </summary>
    public const string ExportBlobContainer = "data-exports";

    /// <summary>
    /// Path of the in-app privacy page on the TMS portal, used for the link
    /// in the data-export-ready email.
    /// </summary>
    public const string PortalPrivacyPath = "/settings/privacy";
}
