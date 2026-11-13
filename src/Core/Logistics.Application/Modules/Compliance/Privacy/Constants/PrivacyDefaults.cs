namespace Logistics.Application.Modules.Compliance.Privacy.Constants;

/// <summary>System-wide defaults for GDPR data subject rights and retention.</summary>
public static class PrivacyDefaults
{
    /// <summary>After this, the daily DataExportExpiryService deletes the ZIP and marks the request Expired.</summary>
    public static readonly TimeSpan ExportBlobRetention = TimeSpan.FromDays(7);

    /// <summary>Short-lived because the portal regenerates the URL on demand; emails never carry it.</summary>
    public static readonly TimeSpan ExportSignedUrlLifetime = TimeSpan.FromHours(1);

    /// <summary>Wait between a deletion request and irreversible anonymization.</summary>
    public static readonly TimeSpan DeletionGracePeriod = TimeSpan.FromDays(30);

    /// <summary>The retention job purges older notifications, read or not.</summary>
    public static readonly TimeSpan NotificationRetention = TimeSpan.FromDays(365);

    /// <summary>The retention job purges older dispatch session transcripts.</summary>
    public static readonly TimeSpan AIDispatchSessionRetention = TimeSpan.FromDays(730);

    /// <summary>Per user per 24h.</summary>
    public const int ExportRateLimitPerDay = 1;

    /// <summary>Lower-case, no underscores, to satisfy Azure Blob container-name rules.</summary>
    public const string ExportBlobContainer = "data-exports";

    /// <summary>One IdentityServer page serves every portal, so the export email links here.</summary>
    public const string PrivacyPagePath = "/account/manage/privacy";
}
