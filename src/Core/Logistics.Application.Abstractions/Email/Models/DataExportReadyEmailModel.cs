namespace Logistics.Application.Abstractions.Email.Models;

public class DataExportReadyEmailModel
{
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Link to the in-app privacy page where the user can fetch a fresh
    /// short-lived signed URL for their export. We never send the signed
    /// URL itself in email.
    /// </summary>
    public string PortalUrl { get; set; } = string.Empty;

    /// <summary>
    /// Human-formatted date when the export blob is deleted.
    /// </summary>
    public string ExpiresAt { get; set; } = string.Empty;
}
